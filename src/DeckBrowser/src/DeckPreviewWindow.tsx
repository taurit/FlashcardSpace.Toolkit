import { useEffect, useState } from "react";
import "./DeckPreviewWindow.scss";
import DesktopWindow from "./DesktopWindow/DesktopWindow";
import AudioPlayer from "./Elements/AudioPlayer";
import { FlashcardDeck } from "./models/FlashcardDeck";

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

    return (
        <section className="flashcards-demo-container">
            <DesktopWindow
                windowClassName="anki-window-frame"
                mainContent={
                    <div className="flashcard">
                        <div className="question">{data.flashcards![flashcardIndex].term}</div>

                        <AudioPlayer uniqueId="questionAudio" pathToAudioFile={`./${deckName}/${data.flashcards![flashcardIndex].termAudio}`} />

                        <hr />
                        <div className="answer">{data.flashcards![flashcardIndex].termTranslation}</div>

                        <AudioPlayer uniqueId="answerAudio" pathToAudioFile={`./${deckName}/${data.flashcards![flashcardIndex].termTranslationAudio}`} />

                        <img src={`./${deckName}/${data.flashcards![flashcardIndex].imageCandidates![0]}`} alt="" className="illustration" />
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
