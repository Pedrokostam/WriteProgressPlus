using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace WriteProgressPlus.P5Tester
{
    public class ItemFormatter
    {
        static readonly string ScriptParamName = "DisplayScript";
        static readonly string PropertyParamName = "DisplayProperties";
        static readonly string SeparatorParamName = "DisplayPropertiesSeparator";
        public ScriptBlock Script { get; set; }
        public string[] Properties { get; set; }
        public string PropertiesSeparator { get; set; }

        public void Update(ScriptBlock script, string[] props, string sep)
        {
            if (script is null) Script = null;
            else
            {
                string scs = script.ToString();
                if (scs.Contains("$_"))
                {
                    scs = scs.Replace("$_", "$args[0]");
                    Script = ScriptBlock.Create(scs);
                }
                else
                {
                    Script = script;
                }

            }
            Properties = props;
            PropertiesSeparator = sep ?? " ";
        }
        public string FormatItem(object item)
        {
            if (item is null) return null;
            if (Script != null)
            {
                return Script.InvokeReturnAsIs(item).ToString();
            }
            else if (Properties != null && Properties.Length > 0)
            {
                List<object> vals = new List<object>();
                if (item is PSObject pso)
                {
                    vals = GetPropertyOfPsObject(pso, Properties);
                }
                else
                {
                    vals = GetPropertyOfNormalObject(item, Properties);

                }
                return string.Join(PropertiesSeparator, vals.ToArray());
            }
            else
            {
                return item.ToString();
            }
        }
        List<object> GetPropertyOfPsObject(PSObject pso, string[] props)
        {
            List<object> oo = new List<object>();
            PSMemberInfoCollection<PSPropertyInfo> objectProps = pso.Properties;
            foreach (string name in props)
            {
                foreach (var x in objectProps.Match(name))
                {
                    if (x != null)
                        oo.Add(x.Value);
                }
            }
            return oo;
        }
        List<object> GetPropertyOfNormalObject(object obj, string[] props)
        {
            List<object> oo = new List<object>();
            Type t = obj.GetType();
            List<string> l = new List<string>(Properties.Length);
            var allprops = t.GetProperties();
            foreach (string name in Properties)
            {
                if (Regex.IsMatch(name, @"[\*\?]"))
                {
                    MatchEvaluator eval = new MatchEvaluator(ReplaceWildcard);
                    string zx = Regex.Escape(name);
                    string pattern = Regex.Replace(Regex.Escape(name), @"(\\\*)|(\\\?)", eval);
                    foreach (var oprop in allprops)
                    {
                        if (Regex.IsMatch(oprop.Name, pattern, RegexOptions.IgnoreCase))
                        {
                            oo.Add(oprop.GetValue(obj));
                        }
                    }
                }
                else
                {
                    foreach (var oprop in allprops)
                    {
                        if (oprop.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            oo.Add(oprop.GetValue(obj));
                        }
                    }
                }
            }
            return oo;
        }
        private string ReplaceWildcard(Match m)
        {
            if (m.Value == @"\*")
                return ".*";
            else
                return ".";
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "FORMAT")]
    [CmdletBinding(PositionalBinding = false)]

    public class WriteProgressPlus : PSCmdlet
    {
        [Parameter()]
        public object InputObject { get; set; }

        [Parameter()]
        public ScriptBlock DisplayScript { get; set; }
        [Parameter()]
        public string[] DisplayProperties { get; set; }
        [Parameter()]
        [ValidateNotNull]
        public string DisplayPropertiesSeparator { get; set; } = ", ";

        private readonly ItemFormatter Formatter = new ItemFormatter();
        protected override void BeginProcessing()
        {
            Formatter.Update(DisplayScript, DisplayProperties, DisplayPropertiesSeparator);
        }
        protected override void ProcessRecord()
        {
            WriteObject(Formatter.FormatItem(InputObject));
        }
    }
}