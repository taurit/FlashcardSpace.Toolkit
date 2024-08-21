namespace BookToAnki.Models;

// Don't rename or add new properties! They are serialized and stored in database.
[Serializable]
public record AudioShift(TimeSpan TimeShiftBeginning, TimeSpan TimeShiftEnd);
