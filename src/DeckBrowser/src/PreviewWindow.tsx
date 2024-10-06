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
                        <div className="question">{flashcard.term}</div>

                        <AudioPlayer uniqueId="questionAudio" pathToAudioFile={`./${deckName}/${flashcard.termAudio}`} />

                        <hr />
                        <div className="answer">{flashcard.termTranslation}</div>

                        <AudioPlayer uniqueId="answerAudio" pathToAudioFile={`./${deckName}/${flashcard.termTranslationAudio}`} />

                        <Image deckName={deckName} imageCandidates={flashcard.imageCandidates!} selectedImageIndex={flashcard.selectedImageIndex} />

                        {/* <div className="definition">{flashcard.termDefinition}</div> */}
                        <div className="usageExample">
                            <AudioPlayer
                                uniqueId="exampleAudio"
                                pathToAudioFile={`./${deckName}/${flashcard.contextAudio}`}
                                replaceDefaultIconWith={<div className="sentenceExample">{flashcard.context}</div>}
                            />

                            <div className="sentenceExampleTranslated">{flashcard.contextTranslation}</div>
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
