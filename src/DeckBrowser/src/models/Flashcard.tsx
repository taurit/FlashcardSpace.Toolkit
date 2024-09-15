export class Flashcard {
    public term: string | undefined;
    public termAudio: string | undefined;

    public termTranslation: string | undefined;
    public termTranslationAudio: string | undefined;

    public termDefinition: string | undefined;

    public context: string | undefined;
    public contextTranslation: string | undefined;

    public type: string | undefined;
    public imageCandidates: string[] | undefined;
}
