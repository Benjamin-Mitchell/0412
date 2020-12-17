using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Value
{
	public enum Denotation { FILLER, k, m, b, t, q, Q, s, S, o, n, d, T, Qu };

	//integer value 0-999
	[SerializeField]
	private float val;

	[SerializeField]
	private Denotation denotation;

	public static implicit operator Value(float f)
	{
		Denotation d = 0;
		float v = f;
		while(v >= 1000.0f)
		{
			v /= 1000.0f;
			d++;
		}
		return new Value() { val = v, denotation = d};
	}

	public Value(){ val = 0; denotation = 0; }

	public Value(float a, Denotation b)
	{
		val = a;
		denotation = b;
	}

	public static Value operator +(Value a, Value b)
	{
		Value c = 0;
		int denotationDiff = (int)a.denotation - (int)b.denotation;

		if (denotationDiff == 0)
		{
			c.val = a.val + b.val;
			c.denotation = a.denotation;
		}
		else if (denotationDiff > 0 && denotationDiff < 3) //base value is much higher than input value.
		{
			c.val = a.val + (b.val / Mathf.Pow(1000, denotationDiff));
			c.denotation = a.denotation;
		}
		else if (denotationDiff < 0 && denotationDiff > -3) // base value is much lower than input value.
		{
			c.val = b.val; //10
			c.val += (a.val / Mathf.Pow(1000, Mathf.Abs(denotationDiff))); //120/1000 = 0.12
			c.denotation = b.denotation;
		}
		else if (denotationDiff <= -3)// base value is way lower than input value, replace with input value.
		{
			c.val = b.val;
			c.denotation = b.denotation;
		}

		while(c.val > 1000.0f)
		{
			c.val /= 1000.0f;
			c.denotation++;
		}

		return c;
	}

	

	public static Value operator -(Value a, Value b)
	{
		Value c = 0;
		c.denotation = a.denotation;
		int denotationDiff = (int)a.denotation - (int)b.denotation;

		if (denotationDiff == 0)
			c.val = a.val - b.val;
		else if (denotationDiff > 0 && denotationDiff < 3)
			c.val = a.val - (b.val / Mathf.Pow(1000, denotationDiff));
		else if (denotationDiff < 0)
		{
			c.val = 0;
			c.denotation = 0;
		}

		//this can only happen once for subtraction
		if(c.val < 0)
		{
			if(c.denotation > 0)
			{
				c.val += 1000.0f;
				c.denotation--;
			}
			else
			{
				c.val = 0;
				c.denotation = 0;
			}
		}
		else if (c.val < 1)
		{
			if (c.denotation > 0)
			{
				c.val *= 1000.0f;
				c.denotation--;
			}
		}



		return c;
	}

	public static Value operator *(Value a, float b)
	{
		Value c = 0;
		c.val = a.val * b;
		c.denotation = a.denotation;

		while (c.val > 1000.0f)
		{
			c.val /= 1000.0f;
			c.denotation++;
		}

		return c;
	}

	public static Value operator *(Value a, Value b)
	{
		Value c = 0;
		c.val = a.val * b.val;
		c.denotation = a.denotation + (int)b.denotation;
		

		while (c.val < 1.0f && c.denotation > 0)
		{
			c.val *= 1000.0f;
			c.denotation--;
		}

		return c;
	}

	public static Value operator /(Value a, float b)
	{
		Value c = 0;
		c.val = a.val / b;
		c.denotation = a.denotation;

		while (c.val < 1.0f && c.denotation > 0)
		{
			c.val *= 1000.0f;
			c.denotation--;
		}

		return c;
	}

	//test: base value is:
	//val: 10		   
	//denotation: 0   
	//				   
	//input value is:  
	//val: 1		   
	//denotation: 1	   
	//				   
	//output:		   
	//val:1 		   
	//denotation:1 	   

	public static Value operator /(Value a, Value b)
	{
		Value c = 0;
		c.denotation = a.denotation - (int)b.denotation; // -1

		c.val = a.val / b.val;

		while(c.denotation < 0)
		{
			c.val /= (1000 * ((int)c.denotation * -1));
			c.denotation++;
		}

		return c;
	}

	public static bool operator >(Value a, Value b)
	{
		if (a.denotation > b.denotation)
			return true;

		if (a.denotation == b.denotation && a.val > b.val)
			return true;

		return false;
	}

	public static bool operator <(Value a, Value b)
	{
		if (a.denotation < b.denotation)
			return true;
		
		if(a.denotation == b.denotation && a.val < b.val)
			return true;

		return false;
	}

	public static Value RandomRange(Value a, Value b)
	{
		Value c = 0;

		int denotationDiff = (int)a.denotation - (int)b.denotation;
		
		if (denotationDiff == 0)
		{
			c.val = UnityEngine.Random.Range(a.val, b.val);
			c.denotation = a.denotation;
		}
		else
		{
			Value greater = denotationDiff > 0 ? a : b;
			Value lower = denotationDiff > 0 ? b : a;

			c.denotation = (Denotation)UnityEngine.Random.Range((int)lower.denotation, (int)greater.denotation + 1);

			if (c.denotation == greater.denotation)
				c.val = UnityEngine.Random.Range(0, greater.val);
			else if (c.denotation == lower.denotation)
				c.val = UnityEngine.Random.Range(lower.val, 999.0f);
			else
				c.val = UnityEngine.Random.Range(0, 999.0f);
		}

		return c;
	}

	public static Value Pow(Value a, float b)
	{
		Value c = 0;

		c.val = Mathf.Pow(a.val, b);

		while (c.val > 1000.0f)
		{
			c.val /= 1000.0f;
			c.denotation++;
		}

		return c;
	}

	//This will do its best to convert a Value back into a float.
	//Use for maths (e.g. calculating the percentage one Value is of another)
	public float ToFloat()
	{
		//its really big, make it a double.
		if((int)denotation > 13)
			throw new Exception("This Value is too big to fit into a float! Use ToDouble instead!");

		float d = val * (denotation > 0 ? (Mathf.Pow(1000.0f, (float)denotation)) : 1);
		
		return d;
	}

	public string GetStringVal()
	{
		string s = (val).ToString();
		int slength = s.Length;
		int maxLength = 3;

		if (s.Contains(".") && slength > 3)
		{
			if(s[3] != '.')
				maxLength = 4;
		}

		slength = slength > maxLength ? maxLength : slength;

		return (s.Substring(0, slength)) + 
			((int)denotation > 0 ? denotation.ToString() : "");
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
