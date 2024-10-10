import { useState } from "react";
import DesktopWindow from "./DesktopWindow/DesktopWindow";
import AudioPlayer from "./Elements/AudioPlayer";
import Image from "./Elements/Image";
import "./PreviewWindow.scss";
import { FlashcardDeck } from "./models/FlashcardDeck";

interface DeckPreviewWindowProps {
    deck: FlashcardDeck | null;
}

const DeckPreviewWindow: React.FC<DeckPreviewWindowProps> = ({ deck }) => {
    const deckName = "FlashcardDeck";

    const [flashcardIndex, setFlashcardIndex] = useState(0);

    // display the flashcard data
    if (!deck) return null;
    const numFlashcards = deck.flashcards?.length ?? 0;

    const goToPreviousExample = () => setFlashcardIndex((index) => (((index - 1) % numFlashcards) + numFlashcards) % numFlashcards);
    const goToNextExample = () => setFlashcardIndex((index) => (index + 1) % numFlashcards);

    const flashcard = deck.flashcards![flashcardIndex];
    const hasMoreThanOneFlashcard = numFlashcards > 1;
    return (
        <section className="flashcards-demo-container">
            <DesktopWindow
                windowClassName="anki-window-frame"
                mainContent={
                    <div className="flashcard">
                        <div className="question">{(flashcard.overrides ? flashcard.overrides.term : null) ?? flashcard.term}</div>

                        <AudioPlayer
                            uniqueId="questionAudio"
                            pathToAudioFile={`./${deckName}/${(flashcard.overrides ? flashcard.overrides.termAudio : null) ?? flashcard.termAudio}`}
                        />

                        <hr />
                        <div className="answer">{(flashcard.overrides ? flashcard.overrides.termTranslation : null) ?? flashcard.termTranslation}</div>

                        <AudioPlayer
                            uniqueId="answerAudio"
                            pathToAudioFile={`./${deckName}/${
                                (flashcard.overrides ? flashcard.overrides.termTranslationAudio : null) ?? flashcard.termTranslationAudio
                            }`}
                        />

                        <Image
                            deckName={deckName}
                            imageCandidates={flashcard.imageCandidates!}
                            selectedImageIndex={(flashcard.overrides ? flashcard.overrides.selectedImageIndex : null) ?? flashcard.selectedImageIndex}
                        />

                        {/* <div className="definition">{flashcard.termDefinition}</div> */}
                        <div className="usageExample">
                            <AudioPlayer
                                uniqueId="exampleAudio"
                                pathToAudioFile={`./${deckName}/${(flashcard.overrides ? flashcard.overrides.contextAudio : null) ?? flashcard.contextAudio}`}
                                replaceDefaultIconWith={
                                    <div className="sentenceExample">{(flashcard.overrides ? flashcard.overrides.context : null) ?? flashcard.context}</div>
                                }
                            />

                            <AudioPlayer
                                uniqueId="exampleAudioTranslation"
                                pathToAudioFile={`./${deckName}/${
                                    (flashcard.overrides ? flashcard.overrides.contextTranslationAudio : null) ?? flashcard.contextTranslationAudio
                                }`}
                                replaceDefaultIconWith={
                                    <div className="sentenceExampleTranslated">
                                        {(flashcard.overrides ? flashcard.overrides.contextTranslation : null) ?? flashcard.contextTranslation}
                                    </div>
                                }
                            />
                        </div>
                    </div>
                }
                bottomContent={
                    hasMoreThanOneFlashcard && (
                        <>
                            <div className="button bottomButton previousExample" onClick={goToPreviousExample}>
                                Previous
                            </div>
                            <div className="button bottomButton nextExample" onClick={goToNextExample}>
                                Next
                            </div>
                        </>
                    )
                }
            />
        </section>
    );
};

export default DeckPreviewWindow;
