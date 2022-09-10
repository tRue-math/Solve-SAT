using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{
    public Text TextFormula;
	
	public Text TextTime;
	
	public Text TextScore;

	public int FormulaLen;

	public int FormulaChars;

	public float TimeLimit;

	[SerializeField] List<Toggle> toggles;

	[SerializeField] GameObject ImageCorrect, ImageIncorrect;

	private string formula;

	private float countup;

	public static int Score;

	public int PlusScore;

	public int MinusScore;

	//0なら出題中
	//1なら結果出力中
	private int GameMode;

	private float tmptime;

	void Start()
    {
		formula = SetChars(formula = MakeSAT(FormulaLen), FormulaLen, FormulaChars);
		TextFormula.text = formula;
		countup = 0.0f;
		Score = 0;
		GameMode = 0;
	}

	void Update()
    {
		countup += Time.deltaTime;

		TextTime.text = "TIME:" + (((int)TimeLimit - (int)countup) / 60).ToString() + ":" + (((int)TimeLimit - (int)countup) % 60).ToString("00");
		TextScore.text = "SCORE:" + Score.ToString();

		if (GameMode == 1 && tmptime + 1.0f <= countup)
		{
			GameMode = 0;
			ImageCorrect.SetActive(false);
			ImageIncorrect.SetActive(false);
			formula = SetChars(formula = MakeSAT(FormulaLen), FormulaLen, FormulaChars);
			TextFormula.text = formula;
			for(int i = 0; i < FormulaChars; i++)
            {
				toggles[i].isOn = false;
            }
		}

        if (TimeLimit <= countup)
        {
			SceneManager.LoadScene("Result");
		}

		if (Input.GetKey(KeyCode.Escape))
        {
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
			Application.Quit();//ゲームプレイ終了
#endif
		}
	}

	public void OnAnswer()
    {
		if (SATCheck(SetBool(formula, -1))) 
        {
			ImageCorrect.SetActive(true);
			Score += PlusScore;
        }
        else
        {
			ImageIncorrect.SetActive(true);
			Score -= MinusScore;
        }
		GameMode = 1;
		tmptime = countup;
		return;
    }

	public void OnImpossible()
    {
		bool tmp = true;
		for(int i = 0; i < (1 << FormulaChars); i++)
        {
            if (SATCheck(SetBool(formula, i)))
            {
				tmp = false;
            }
        }
        if (tmp)
        {
            ImageCorrect.SetActive(true);
            Score += PlusScore;
        }
        else
        {
            ImageIncorrect.SetActive(true);
			Score -= MinusScore * 2;
        }
		GameMode = 1;
		tmptime = countup;
        return;
    }

	string SetBool(string f, int bit)
	{
        if (bit >= 0)
        {
			for (int i = 0; i < f.Length; i++)
			{
				if ('A' <= f[i] && f[i] < 'A' + FormulaChars)
				{
					if ((bit & (1 << (f[i] - 'A'))) != 0)
					{
						f = f.Substring(0, i) + "T" + f.Substring(i + 1, f.Length - i - 1);
					}
					else
					{
						f = f.Substring(0, i) + "F" + f.Substring(i + 1, f.Length - i - 1);
					}
				}
			}
			return f;
		}
		for (int i = 0; i < f.Length; i++)
		{
			if ('A' <= f[i] && f[i] < 'A' + FormulaChars)
			{
				if (toggles[f[i] - 'A'].isOn)
                {
					f = f.Substring(0, i) + "T" + f.Substring(i + 1, f.Length - i - 1);
                }
                else
                {
					f = f.Substring(0, i) + "F" + f.Substring(i + 1, f.Length - i - 1);
				}
			}
		}
		return f;
	}

	bool SATCheck(string f)
	{
		Stack<char> ans = new Stack<char>();
		Stack<char> que = new Stack<char>();

		for (int i = f.Length - 1; i >= 0; i--)
		{
			que.Push(f[i]);
		}

		while (que.Count != 0)
		{
			if (ans.Count == 0)
			{
				ans.Push(que.Pop());
				continue;
			}
			if (ans.Peek() == '￢' && (que.Peek() == 'T' || que.Peek() == 'F'))
			{
				ans.Pop();
				if (que.Pop() == 'T') que.Push('F');
				else que.Push('T');
				continue;
			}
			if ((ans.Peek() == '∧' || ans.Peek() == '∨') && (que.Peek() == 'T' || que.Peek() == 'F'))
			{
				if (ans.Pop() == '∧')
				{
					bool tmp = ('T' == ans.Pop()) & ('T' == que.Pop());
					if (tmp) que.Push('T');
					else que.Push('F');
				}
				else
				{
					bool tmp = ('T' == ans.Pop()) | ('T' == que.Pop());
					if (tmp) que.Push('T');
					else que.Push('F');
				}
				continue;
			}
			if (que.Peek() == ')')
			{
				que.Pop(); que.Push(ans.Pop()); ans.Pop();
				continue;
			}
			ans.Push(que.Pop());
		}
        if (ans.Count != 1)
        {
			string tmp = "";
			while (ans.Count != 0) tmp = ans.Pop() + tmp;
			Debug.Log(tmp);
			return tmp[tmp.Length - 1] == 'T';
        }
		return ans.Pop() == 'T';
	}

	string SetChars(string f, int len, int chars)
    {
		int[] tmp = new int[len];
		for (int i = 0; i < chars; i++) tmp[i] = i;
		for (int i = chars; i < len; i++) tmp[i] = Random.Range(0, chars);
		for (int i = 0; i < len; i++)
		{
			int a = Random.Range(0, len), b = Random.Range(0, len), c;
			c = tmp[a]; tmp[a] = tmp[b]; tmp[b] = c;
		}

		int cnt = 0;
		for(int i = 0; i < f.Length; i++)
        {
            if (f[i] == 'X')
            {
				f = f.Substring(0, i) + (char)('A' + tmp[cnt]) + f.Substring(i + 1, f.Length - i - 1);
				cnt++;
            }
        }
		return f.ToString();
	}

	string MakeSAT(int n)
	{
		string ans = "";
		if (n == 1)
		{
			if ((int)(Random.Range(0, 2)) != 0)
			{
				ans += "￢";
			}
			ans += "X";
			return ans;
		}
		int rand = ((int)(Random.Range(0, 1 << (n - 1))) + 1) % (1 << (n - 1));
		string andor;
		if ((int)(Random.Range(0, 4)) != 0) andor = "∧";
		else andor = "∨";
		int cnt = 1;
		for (int i = 0; i < n - 1; i++)
		{
			if ((rand & (1 << i)) != 0)
			{
				if (cnt == 1)
				{
					ans += MakeSAT(cnt);
				}
				else
				{
					if ((int)(Random.Range(0, 2)) != 0)
					{
						ans += "￢";
					}
					ans += "(" + MakeSAT(cnt) + ")";
				}
				ans += andor;
				cnt = 1;
			}
			else cnt++;
		}
		if (cnt == 1 || cnt == n)
		{
			ans += MakeSAT(cnt);
		}
		else
		{
			ans += "(" + MakeSAT(cnt) + ")";
		}
		return ans;
	}
}
