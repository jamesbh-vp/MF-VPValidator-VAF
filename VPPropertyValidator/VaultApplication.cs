using MFiles.VAF;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Core;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VPPropertyValidator;

namespace VPPropertyValidator
{

    public class VaultApplication : ConfigurableVaultApplicationBase<Configuration>
    {
        [EventHandler(MFEventHandlerType.MFEventHandlerBeforeCheckInChangesFinalize)]
        public void ValidateProperties(EventHandlerEnvironment env)
        {
            if (Configuration?.ValidationRules == null || Configuration.ValidationRules.Count == 0)
                return;

            var obj = new ObjVerEx(env.Vault, env.ObjVer);
            var validationFailures = new List<string>();

            // Optimization: Get the ID of the current object's class once.
            int currentClassId = obj.GetProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass).TypedValue.GetLookupID();

            foreach (var rule in Configuration.ValidationRules)
            {
                // --- NEW LOGIC: LIST FILTERING ---

                // 1. Check if the rule has specific Target Classes defined.
                // We check for null AND .Any() because the list might exist but be empty.
                if (rule.TargetClasses != null && rule.TargetClasses.Any())
                {
                    // 2. If the list is NOT empty, we must enforce it.
                    // Check if the current object's Class ID is in the list of allowed classes.
                    bool isClassMatch = rule.TargetClasses.Any(c => c.ID == currentClassId);

                    // 3. If the current class is NOT in the allowed list, SKIP this rule.
                    if (!isClassMatch)
                    {
                        continue;
                    }
                }
                // If the list was empty/null, we fall through here (Global Rule behavior)
                // --- END NEW LOGIC ---

                // Proceed with Property check
                var propId = rule.PropertyDef.ID;
                if (obj.HasProperty(propId))
                {
                    string propValue = obj.GetProperty(propId).GetValueAsLocalizedText();

                    if (!IsValueValid(propValue, rule.RegexPattern))
                    {
                        string propName = env.Vault.PropertyDefOperations.GetPropertyDef(propId).Name;
                        validationFailures.Add($"• {propName}: {rule.ValidationMessage}");
                    }
                }
            }

            if (validationFailures.Any())
            {
                throw new System.Exception(BuildErrorMessage(validationFailures));
            }
        }

        /// <summary>
        /// Helper to format a list of error strings into a clean message for the M-Files UI.
        /// </summary>
        private string BuildErrorMessage(List<string> failures)
        {
            var messageBuilder = new StringBuilder();

            messageBuilder.AppendLine("The document cannot be saved due to the following validation errors:");
            messageBuilder.AppendLine(); // Blank line for spacing

            foreach (var failure in failures)
            {
                messageBuilder.AppendLine(failure);
            }

            messageBuilder.AppendLine(); // Blank line
            messageBuilder.AppendLine("Please correct these values and try again.");

            return messageBuilder.ToString();
        }

        /// <summary>
        /// Helper to run the Regex safely.
        /// </summary>
        private bool IsValueValid(string input, string pattern)
        {
            // If input is empty, we consider it valid (unless we want to enforce mandatory fields, 
            // which is usually better handled by M-Files 'Mandatory' property setting).
            if (string.IsNullOrWhiteSpace(input)) return true;

            // If pattern is empty, we cannot validate, so we assume valid.
            if (string.IsNullOrWhiteSpace(pattern)) return true;

            try
            {
                return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException ex)
            {
                // Log invalid regex configuration to Windows Event Log so Admins know to fix it.
                SysUtils.ReportErrorMessageToEventLog($"Invalid Regex pattern: {pattern}", ex);
                return true; // Fail open to avoid blocking users due to config error
            }
        }
    }

}