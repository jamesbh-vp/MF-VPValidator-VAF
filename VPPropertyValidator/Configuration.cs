using MFiles.VAF.Configuration;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MFiles.VAF.Configuration.JsonAdaptor;
using System;

namespace VPPropertyValidator
{
   

    [DataContract]
    public class Configuration
    {
        [DataMember]
        [JsonConfEditor(Label = "Validation Rules", HelpText = "Define regex validations for specific properties.")]
        public List<ValidationRule> ValidationRules { get; set; } = new List<ValidationRule>();
    }

    [DataContract]
    public class ValidationRule
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        [JsonConfEditor(Label = "Target Classes", 
            HelpText = "Leave empty to apply to ALL classes.",
            IsRequired = false, 
            DefaultValue =null)]
        [MFClass] // Creates a dropdown of Classes
        public List<MFIdentifier> TargetClasses { get; set; }

        [DataMember]
        [JsonConfEditor(Label = "Property to Validate")]
        [MFPropertyDef]
        public MFIdentifier PropertyDef { get; set; }

        [DataMember]
        [JsonConfEditor(Label = "Regex Pattern")]
        public string RegexPattern { get; set; }

        [DataMember]
        [JsonConfEditor(Label = "Error Message")]
        public string ValidationMessage { get; set; } = "The value format is invalid.";
    }
}