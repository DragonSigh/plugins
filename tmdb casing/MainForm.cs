﻿using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse
{
    public partial class MainForm : Form
    {
        public string FixedSubtitle { get; private set; }
        private string _name;
        private string _description;
        private Subtitle _subtitle;
        private Form _subtitleEditForm;
        private List<string> Characters;

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(Subtitle sub, string name, string description, Form parentForm, List<string> charsts)
            : this()
        {
            // TODO: Complete member initialization
            this._subtitle = sub;
            this._name = name;
            this._description = description;
            this._subtitleEditForm = parentForm;
            this.Characters = charsts;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            FillListView();
            GeneratePreview();
        }

        private void FillListView()
        {
            if (_subtitle.Paragraphs.Count < 1)
                throw new InvalidOperationException();
            //foreach (Paragraph p in _subtitle.Paragraphs)
            //{
            //    string after = p.Text;
            //    AddtoListViewParagraps(p, after);
            //}
            //Application.DoEvents();

            if (this.Characters.Count > 0)
                foreach (var name in Characters)
                {
                    AddToListViewNames(name);
                }
        }

        private void AddtoListViewParagraps(Paragraph p, string after)
        {
            var item = new ListViewItem() { Tag = p, Checked = true };
            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, p.Text);
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, after);
            item.SubItems.Add(subItem);
            listViewParagraphs.Items.Add(item);
        }

        private void AddToPreviewListView(Paragraph p, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, newText.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            listViewParagraphs.Items.Add(item);
        }

        private void AddToListViewNames(string name)
        {
            var item = new ListViewItem() { Checked = true };
            var subItem = new ListViewItem.ListViewSubItem(item, name);
            item.SubItems.Add(subItem);
            listViewNames.Items.Add(item);
            //SetText(item);
        }

        private void GeneratePreview()
        {
            Cursor = Cursors.WaitCursor;
            listViewParagraphs.BeginUpdate();
            listViewParagraphs.Items.Clear();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                string text = p.Text;
                foreach (ListViewItem item in listViewNames.Items)
                {
                    string name = item.SubItems[1].Text.Trim();
                    string textNoTags = Utilities.RemoveHtmlTags(text);
                    if (textNoTags != textNoTags.ToUpper())
                    {
                        if (item.Checked && text != null && text.ToLower().Contains(name.ToLower()) && name.Length > 1 && name != name.ToLower())
                        {
                            var st = new StripableText(text);
                            st.FixCasing(new List<string> { name }, true, false, false, string.Empty);
                            text = st.MergedString;
                        }
                    }
                }
                if (text != p.Text)
                    AddToPreviewListView(p, text);
            }
            listViewParagraphs.EndUpdate();
            Cursor = Cursors.Default;
        }

        private void listViewNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewNames.SelectedItems.Count != 1)
                return;

            string name = listViewNames.SelectedItems[0].SubItems[1].Text;
            listViewParagraphs.BeginUpdate();
            foreach (ListViewItem item in listViewParagraphs.Items)
            {
                item.Selected = false;

                string text = item.SubItems[2].Text.Replace(Configuration.ListViewLineSeparatorString, Environment.NewLine);

                string lower = text.ToLower();
                if (lower.Contains(name.ToLower()) && name.Length > 1 && name != name.ToLower())
                {
                    int start = lower.IndexOf(name.ToLower());
                    if (start >= 0)
                    {
                        bool startOk = (start == 0) || (lower[start - 1] == ' ') || (lower[start - 1] == '-') || (lower[start - 1] == '"') ||
                                       (lower[start - 1] == '\'') || (lower[start - 1] == '>') || (Environment.NewLine.EndsWith(lower[start - 1].ToString()));

                        if (startOk)
                        {
                            int end = start + name.Length;
                            bool endOk = end <= lower.Length;
                            if (endOk)
                                endOk = end == lower.Length || (" ,.!?:;')<-\"" + Environment.NewLine).Contains(lower[end].ToString());
                            item.Selected = endOk;
                        }
                    }
                }
            }
            listViewParagraphs.EndUpdate();
            if (listViewParagraphs.SelectedItems.Count > 0)
                listViewParagraphs.EnsureVisible(listViewParagraphs.SelectedItems[0].Index);
        }
    }
}