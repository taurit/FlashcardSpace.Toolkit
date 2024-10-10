import { FlashcardNoteEditablePart } from "./FlashcardNoteEditablePart";

export class FlashcardNote extends FlashcardNoteEditablePart {
    public approvalStatus: "NotReviewedYet" | "Approved" | "Rejected" | undefined;
    public contextEnglishTranslation: string | undefined;
    public imageCandidates: string[] | undefined;
    public termBaseForm: string | undefined;
    public termEnglishTranslation: string | undefined;
    public termDefinition: string | undefined;
    public type: string | undefined;
    public overrides: FlashcardNoteEditablePart | undefined;
}
