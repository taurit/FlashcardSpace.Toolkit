namespace CoreLibrary.Interfaces;

public interface IProvideFieldValues
{
    Task<List<Note>> ProcessNotes(List<Note> notes);
}
