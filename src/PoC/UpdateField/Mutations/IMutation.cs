using AnkiCardValidator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateField.Mutations;
internal interface IMutation
{
    List<AnkiNote> LoadNotesThatRequireAdjustment();
    Task RunMigration(List<AnkiNote> notes);
}
