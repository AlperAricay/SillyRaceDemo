using System.Collections;
using System.Collections.Generic;
using PaintIn3D;
using UnityEngine;
using UnityEngine.UI;

public sealed class P3dChangeCounterText : MonoBehaviour
	{
		public List<P3dChangeCounter> Counters { get { if (counters == null) counters = new List<P3dChangeCounter>(); return counters; } } [SerializeField] private List<P3dChangeCounter> counters;
		public bool Inverse { set { inverse = value; } get { return inverse; } } [SerializeField] private bool inverse;
		public int DecimalPlaces { set { decimalPlaces = value; } get { return decimalPlaces; } } [SerializeField] private int decimalPlaces;
		public string Format { set { format = value; } get { return format; } } [Multiline] [SerializeField] private string format = "{PERCENT}";

		[System.NonSerialized]
		private Text cachedText;

		private void OnEnable()
		{
			cachedText = GetComponent<Text>();
		}

		private void Update()
		{
			var finalCounters = counters.Count > 0 ? counters : null;
			var total         = P3dChangeCounter.GetTotal(finalCounters);
			var count         = P3dChangeCounter.GetCount(finalCounters);

			if (inverse == true)
			{
				count = total - count;
			}

			var final   = format;
			var percent = P3dHelper.RatioToPercentage(P3dHelper.Divide(count, total), decimalPlaces);

			final = final.Replace("{TOTAL}", total.ToString());
			final = final.Replace("{COUNT}", count.ToString());
			final = final.Replace("{PERCENT}", percent.ToString());

			cachedText.text = final;
		}
	}
