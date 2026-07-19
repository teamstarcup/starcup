using System.Linq;
using System.Text.Json.Serialization;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Records;

/// <summary>
/// Contains Cosmatic Drift records that can be changed in the character editor. This is stored on the character's profile.
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class PlayerProvidedCharacterRecords
{
    public const int TextMedLen = 64;
    public const int TextVeryLargeLen = 4096;

    /* Basic info */

    // Additional data is fetched from the Profile

    // All
    [DataField]
    public int Height { get; private set; }
    public const int MaxHeight = 800;

    [DataField]
    public int Weight { get; private set; }
    public const int MaxWeight = 300;

    [DataField] // TheDen
    public string Birthday { get; private set; } = "N/A";

    [DataField]
    public string EmergencyContactName { get; private set; }

    // Employment
    [DataField]
    public bool HasWorkAuthorization { get; private set; }

    // Security
    [DataField]
    public string IdentifyingFeatures { get; private set; }

    [DataField] // starcup
    public string SecurityFlags { get; private set; } = "N/A";

    // Medical
    [DataField]
    public string Allergies { get; private set; }
    [DataField]
    public string DrugAllergies { get; private set; }
    [DataField]
    public string PostmortemInstructions { get; private set; }
    // history, prescriptions, etc. would be a record below
    [DataField] // starcup
    public string MedicalNeeds { get; private set; } = "N/A";

    // "incidents"
    [DataField, JsonIgnore]
    public List<RecordEntry> MedicalEntries { get; private set; }
    [DataField, JsonIgnore]
    public List<RecordEntry> SecurityEntries { get; private set; }
    [DataField, JsonIgnore]
    public List<RecordEntry> EmploymentEntries { get; private set; }
    [DataField, JsonIgnore]
    public List<RecordEntry> AdminEntries { get; private set; } = [];

    [DataDefinition]
    [Serializable, NetSerializable]
    public sealed partial class RecordEntry
    {
        [DataField]
        public string Title { get; private set; }
        // players involved, can be left blank (or with a generic "CentCom" etc.) for backstory related issues
        [DataField]
        public string Involved { get; private set; }
        // Longer description of events.
        [DataField]
        public string Description { get; private set; }

        public RecordEntry(string title, string involved, string desc)
        {
            Title = title;
            Involved = involved;
            Description = desc;
        }

        public RecordEntry(RecordEntry other)
        : this(other.Title, other.Involved, other.Description)
        {
        }

        public bool MemberwiseEquals(RecordEntry other)
        {
            return Title == other.Title && Involved == other.Involved && Description == other.Description;
        }

        public void EnsureValid()
        {
            Title = ClampString(Title, TextMedLen);
            Involved = ClampString(Involved, TextMedLen);
            Description = ClampString(Description, TextVeryLargeLen);
        }
    }

    public PlayerProvidedCharacterRecords(
        bool hasWorkAuthorization,
        int height, int weight,
        string birthday, // TheDen
        string emergencyContactName,
        string identifyingFeatures,
        string securityFlags, // starcup
        string allergies, string drugAllergies,
        string postmortemInstructions,
        string medicalNeeds, // starcup
        List<RecordEntry> medicalEntries,
        List<RecordEntry> securityEntries,
        List<RecordEntry> employmentEntries,
        List<RecordEntry> adminEntries)
    {
        HasWorkAuthorization = hasWorkAuthorization;
        Height = height;
        Weight = weight;
        Birthday = birthday; // TheDen
        EmergencyContactName = emergencyContactName;
        IdentifyingFeatures = identifyingFeatures;
        SecurityFlags = securityFlags; // starcup
        Allergies = allergies;
        DrugAllergies = drugAllergies;
        PostmortemInstructions = postmortemInstructions;
        MedicalNeeds = medicalNeeds; // starcup
        MedicalEntries = medicalEntries;
        SecurityEntries = securityEntries;
        EmploymentEntries = employmentEntries;
        AdminEntries = adminEntries;
    }

    public PlayerProvidedCharacterRecords(PlayerProvidedCharacterRecords other)
    {
        Height = other.Height;
        Weight = other.Weight;
        Birthday = other.Birthday; // TheDen
        EmergencyContactName = other.EmergencyContactName;
        HasWorkAuthorization = other.HasWorkAuthorization;
        IdentifyingFeatures = other.IdentifyingFeatures;
        SecurityFlags = other.SecurityFlags; // starcup
        Allergies = other.Allergies;
        DrugAllergies = other.DrugAllergies;
        PostmortemInstructions = other.PostmortemInstructions;
        MedicalNeeds = other.MedicalNeeds; // starcup
        MedicalEntries = other.MedicalEntries.Select(x => new RecordEntry(x)).ToList();
        SecurityEntries = other.SecurityEntries.Select(x => new RecordEntry(x)).ToList();
        EmploymentEntries = other.EmploymentEntries.Select(x => new RecordEntry(x)).ToList();
        AdminEntries = other.AdminEntries.Select(x => new RecordEntry(x)).ToList();
    }

    public static PlayerProvidedCharacterRecords DefaultRecords()
    {
        return new PlayerProvidedCharacterRecords(
            hasWorkAuthorization: true,
            height: 170, weight: 70,
            birthday: "N/A", // TheDen
            emergencyContactName: "",
            identifyingFeatures: "",
            securityFlags: "", // starcup
            allergies: "None",
            drugAllergies: "None",
            postmortemInstructions: "Return home",
            medicalNeeds: "", // starcup
            medicalEntries: new List<RecordEntry>(),
            securityEntries: new List<RecordEntry>(),
            employmentEntries: new List<RecordEntry>(),
            adminEntries: new List<RecordEntry>()
        );
    }

    public bool MemberwiseEquals(PlayerProvidedCharacterRecords other)
    {
        // This is ugly but is only used for integration tests.
        var test = Height == other.Height
                   && Weight == other.Weight
                   && EmergencyContactName == other.EmergencyContactName
                   && Birthday == other.Birthday // TheDen
                   && HasWorkAuthorization == other.HasWorkAuthorization
                   && IdentifyingFeatures == other.IdentifyingFeatures
                   && SecurityFlags == other.SecurityFlags // starcup
                   && Allergies == other.Allergies
                   && DrugAllergies == other.DrugAllergies
                   && PostmortemInstructions == other.PostmortemInstructions
                   && MedicalNeeds == other.MedicalNeeds; // starcup
        if (!test)
            return false;
        if (MedicalEntries.Count != other.MedicalEntries.Count)
            return false;
        if (SecurityEntries.Count != other.SecurityEntries.Count)
            return false;
        if (EmploymentEntries.Count != other.EmploymentEntries.Count)
            return false;
        if (AdminEntries.Count != other.AdminEntries.Count)
            return false;
        if (MedicalEntries.Where((t, i) => !t.MemberwiseEquals(other.MedicalEntries[i])).Any())
        {
            return false;
        }
        if (SecurityEntries.Where((t, i) => !t.MemberwiseEquals(other.SecurityEntries[i])).Any())
        {
            return false;
        }
        if (EmploymentEntries.Where((t, i) => !t.MemberwiseEquals(other.EmploymentEntries[i])).Any())
        {
            return false;
        }

        if (AdminEntries.Where((t, i) => !t.MemberwiseEquals(other.AdminEntries[i])).Any())
        {
            return false;
        }

        return true;
    }

    private static string ClampString(string str, int maxLen)
    {
        if (str.Length > maxLen)
        {
            return str[..maxLen];
        }
        return str;
    }

    private static void EnsureValidEntries(List<RecordEntry> entries)
    {
        foreach (var entry in entries)
        {
            entry.EnsureValid();
        }
    }

    /// <summary>
    /// Clamp invalid entries to valid values
    /// </summary>
    public void EnsureValid()
    {
        Height = Math.Clamp(Height, 0, MaxHeight);
        Weight = Math.Clamp(Weight, 0, MaxWeight);
        Birthday = ClampString(Birthday, TextMedLen); // TheDen
        EmergencyContactName =
            ClampString(EmergencyContactName, TextMedLen);
        IdentifyingFeatures = ClampString(IdentifyingFeatures, TextMedLen);
        SecurityFlags = ClampString(SecurityFlags, TextMedLen); // starcup
        Allergies = ClampString(Allergies, TextMedLen);
        DrugAllergies = ClampString(DrugAllergies, TextMedLen);
        PostmortemInstructions = ClampString(PostmortemInstructions, TextMedLen);
        MedicalNeeds = ClampString(MedicalNeeds, TextMedLen); // starcup

        EnsureValidEntries(EmploymentEntries);
        EnsureValidEntries(MedicalEntries);
        EnsureValidEntries(SecurityEntries);
        EnsureValidEntries(AdminEntries);
    }
    public PlayerProvidedCharacterRecords WithHeight(int height)
    {
        return new(this) { Height = height };
    }
    public PlayerProvidedCharacterRecords WithWeight(int weight)
    {
        return new(this) { Weight = weight };
    }
    public PlayerProvidedCharacterRecords WithWorkAuth(bool auth)
    {
        return new(this) { HasWorkAuthorization = auth };
    }

    public PlayerProvidedCharacterRecords WithBirthday(string birthday) // TheDen
    {
        return new(this) { Birthday = birthday};
    }
    public PlayerProvidedCharacterRecords WithContactName(string name)
    {
        return new(this) { EmergencyContactName = name};
    }
    public PlayerProvidedCharacterRecords WithIdentifyingFeatures(string feat)
    {
        return new(this) { IdentifyingFeatures = feat};
    }

    public PlayerProvidedCharacterRecords WithSecurityFlags(string flags) // starcup
    {
        return new(this) { SecurityFlags = flags};
    }

    public PlayerProvidedCharacterRecords WithAllergies(string s)
    {
        return new(this) { Allergies = s };
    }
    public PlayerProvidedCharacterRecords WithDrugAllergies(string s)
    {
        return new(this) { DrugAllergies = s };
    }

    public PlayerProvidedCharacterRecords WithPostmortemInstructions(string s)
    {
        return new(this) { PostmortemInstructions = s};
    }

    public PlayerProvidedCharacterRecords WithMedicalNeeds(string needs) // starcup
    {
        return new(this) { MedicalNeeds = needs};
    }

    public PlayerProvidedCharacterRecords WithEmploymentEntries(List<RecordEntry> entries)
    {
        return new(this) { EmploymentEntries = entries};
    }
    public PlayerProvidedCharacterRecords WithMedicalEntries(List<RecordEntry> entries)
    {
        return new(this) { MedicalEntries = entries};
    }
    public PlayerProvidedCharacterRecords WithSecurityEntries(List<RecordEntry> entries)
    {
        return new(this) { SecurityEntries = entries};
    }

    public PlayerProvidedCharacterRecords WithAdminEntries(List<RecordEntry> entries)
    {
        return new (this) { AdminEntries = entries };
    }
}

public enum CharacterRecordType : byte
{
    Employment, Medical, Security, Admin,
}
