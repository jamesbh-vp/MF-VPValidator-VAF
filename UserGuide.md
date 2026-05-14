# VP Property Validator — User Guide

**Publisher:** Virtuoso Partners  
**Application version:** 0.2  
**Audience:** M-Files Vault Administrators

---

## What it does

VP Property Validator checks property values on documents at the point of check-in. If a value does not match the configured pattern, the check-in is blocked and the user is shown an error message explaining what needs to be corrected. Rules are configured entirely through the M-Files Admin interface — no code changes are needed.

---

## Configuring validation rules

Open **M-Files Admin**, navigate to your vault, and go to:

> **Applications** → **VP Property Validator** → **Configuration**

You will see a list called **Validation Rules**. Each rule has four fields:

### Target Classes *(optional)*

A list of document classes the rule applies to. **Leave this empty to apply the rule to every class in the vault.** If one or more classes are selected, the rule is only enforced when a document belongs to one of those classes.

### Property to Validate

The property whose value will be checked. Select from the dropdown of all properties defined in the vault.

### Regex Pattern

A regular expression the property value must match. Matching is **case-insensitive**. The pattern is tested against the full value string using standard .NET regex syntax.

**Behavior for edge cases:**

| Situation | Result |
|---|---|
| Property value is empty or whitespace | Passes (not blocked) |
| Regex Pattern field is empty | Passes (not blocked) |
| Pattern is syntactically invalid | Passes with an error logged — see [Troubleshooting](#troubleshooting) |

> To enforce that a field is not empty, use M-Files' built-in **Mandatory** property setting rather than a regex rule.

### Error Message

The text shown to the user when this rule fails. It appears beneath the property name in the error dialog. Defaults to `"The value format is invalid."` if left blank.

---

## Example rules

**UK postcode on a specific class**

| Field | Value |
|---|---|
| Target Classes | `Correspondence` |
| Property to Validate | `Postcode` |
| Regex Pattern | `^[A-Z]{1,2}\d[A-Z\d]?\s?\d[A-Z]{2}$` |
| Error Message | `Must be a valid UK postcode (e.g. SW1A 1AA).` |

**Email address — global rule (all classes)**

| Field | Value |
|---|---|
| Target Classes | *(empty)* |
| Property to Validate | `Contact Email` |
| Regex Pattern | `^[^@\s]+@[^@\s]+\.[^@\s]+$` |
| Error Message | `Must be a valid email address.` |

**Four-digit year**

| Field | Value |
|---|---|
| Target Classes | *(empty)* |
| Property to Validate | `Tax Year` |
| Regex Pattern | `^\d{4}$` |
| Error Message | `Must be a four-digit year (e.g. 2025).` |

---

## What the user sees

When a check-in is blocked, M-Files displays a dialog similar to:

```
The document cannot be saved due to the following validation errors:

• Postcode: Must be a valid UK postcode (e.g. SW1A 1AA).
• Contact Email: Must be a valid email address.

Please correct these values and try again.
```

All failing rules for the document are listed together in a single message so the user can fix them in one go.

---

## Troubleshooting

**A rule never seems to block anything**

- Confirm the **Regex Pattern** field is not empty.
- Test your regex against sample values using a tool such as [regex101.com](https://regex101.com) (select the `.NET` flavour).
- If **Target Classes** is populated, verify the document's class matches one of the listed classes.
- Remember that empty property values always pass — use M-Files Mandatory settings if the field must be filled in.

**An invalid regex pattern was saved by mistake**

The application will not block users in this case — it fails open to avoid disruption. The invalid pattern is recorded as an error in the M-Files Event Log. To view it:

> M-Files Admin → your vault → **Event Log**

Filter by **Source: VPPropertyValidator** or **Level: Error**. Correct the pattern in the configuration and save.

**A rule is applying to classes it shouldn't**

Check that **Target Classes** is not empty. An empty list means the rule is global (applies to all classes). Add the specific classes you intend to target.
