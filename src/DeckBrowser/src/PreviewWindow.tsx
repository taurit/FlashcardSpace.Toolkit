import { useEffect, useState } from "react";
import DesktopWindow from "./DesktopWindow/DesktopWindow";
import AudioPlayer from "./Elements/AudioPlayer";
import Image from "./Elements/Image";
import { FlashcardDeck } from "./models/FlashcardDeck";
import "./PreviewWindow.scss";

function DeckPreviewWindow() {
    const deckName = "FlashcardDeck";

    // fetch flashcard data from the `data.json` file
    const [data, setData] = useState<FlashcardDeck | null>(null);
    const [flashcardIndex, setFlashcardIndex] = useState(0);

    useEffect(() => {
        fetch(`./${deckName}/flashcards.json`)
            .then((response) => response.json())
            .then((data) => setData(data))
            .catch((error) => {
                document.writeln(`An error occurred while fetching the flashcard data: ${error}<br />\n`);
            });
    }, []);

    // display the flashcard data
    if (!data) return null;
    var numFlashcards = data.flashcards?.length ?? 0;

    const goToPreviousExample = () => setFlashcardIndex((index) => (((index - 1) % numFlashcards) + numFlashcards) % numFlashcards);
    const goToNextExample = () => setFlashcardIndex((index) => (index + 1) % numFlashcards);

    const flashcard = data.flashcards![flashcardIndex];
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

                        <Image deckName={deckName} imageCandidates={flashcard.imageCandidates!} />

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
                    <>
                        <div className="button bottomButton previousExample" onClick={goToPreviousExample}>
                            Previous
                        </div>
                        <div className="button bottomButton nextExample" onClick={goToNextExample}>
                            Next
                        </div>
                    </>
                }
            />
        </section>
    );
}

export default DeckPreviewWindow;
