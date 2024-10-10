import { FlashcardNote } from "./FlashcardNote";

export class FlashcardDeck {
    public flashcards: FlashcardNote[] | undefined;
    public deckName: string | undefined;
    public mediaFilesPrefix: string | undefined;
    public sourceLanguage: string | undefined;
    public targetLanguage: string | undefined;
}
