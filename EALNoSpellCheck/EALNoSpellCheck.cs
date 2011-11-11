using System;
using Tomboy;

namespace Tomboy.EALNoSpellCheck
{
	public class EALNoSpellCheck : NoteAddin
	{
	
		NoteTag eal_tag;
		
		public override void Initialize ()
		{
			if (Note.TagTable.Lookup ("lang:eal") == null) {
				eal_tag = new NoteTag ("lang:eal");
				eal_tag.CanUndo = true;
				eal_tag.CanGrow = true; 
				eal_tag.CanSpellCheck = false;
				Note.TagTable.Add (eal_tag);
			} else {
				eal_tag = (NoteTag)Note.TagTable.Lookup ("lang:eal");
			}
		}
	
		public override void Shutdown ()
		{
			// Remove the tag only if we installed it.
			//if (cjk_tag != null)
			//	Note.TagTable.Remove (cjk_tag);        
		}

		public override void OnNoteOpened ()
		{
			Buffer.InsertText += OnInsertText;
			Buffer.DeleteRange += OnDeleteRange;
			
		}

		void OnMenuItemActivated (object sender, EventArgs args)
		{
			// Logger.Log ("Activated 'Insert date and time' menu item");
		}

		void OnInsertText (object sender, Gtk.InsertTextArgs args)
		{                   
			Gtk.TextIter start = args.Pos;
			start.BackwardChars (args.Length);

			ApplyEALToBlock (start, args.Pos);
		}
		
		void OnDeleteRange (object sender, Gtk.DeleteRangeArgs args)
		{
			ApplyEALToBlock (args.Start, args.End);
		}
		
		void ApplyEALToBlock (Gtk.TextIter start, Gtk.TextIter end)
		{
			NoteBuffer.GetBlockExtents (ref start,
                    ref end, 
                    512 /* XXX */,
                    eal_tag);

			Buffer.RemoveTag (eal_tag, start, end);

			MatchEAL m = new MatchEAL (start.GetText (end));
			foreach (MatchEAL.EALGroup g in m) {
				Gtk.TextIter start_cpy = start;
				start_cpy.ForwardChars (g.Start);

				end = start;
				end.ForwardChars (g.End);

				Buffer.ApplyTag (eal_tag, start_cpy, end);
			}
		}
		
		public class MatchEAL : System.Collections.IEnumerable
		{
			string text;

			public MatchEAL(string text)
			{
				this.text = text;
			}

			public System.Collections.IEnumerator GetEnumerator ()
			{
				int cur = 0;
				int start = 0;
				int end = 0;

				while (cur < text.Length) {
					while (cur < text.Length && IsEAL (text[cur]) != true) {
						cur++;
					}
					start = cur;
					while (cur < text.Length && IsEAL (text[cur])) {
						cur++;
					}
					end = cur;
					if (cur <= text.Length)
						yield return new EALGroup(start, end);
				}
			}

			public static bool IsEAL (char c)
			{
				double v = (double)c;
				if (0x2E80 < v && v < 0x2FA1D) /* Range of EAL in unicode */
					return true;
				else
					return false;
			}
			public class EALGroup
			{
				public int Start;
				public int End;

				public EALGroup (int s, int e)
				{
					Start = s;
					End = e;
				}

				public override string ToString ()
				{
					return String.Format ("{0}, {1}", Start, End);
				}
			}
		}


	}
}

