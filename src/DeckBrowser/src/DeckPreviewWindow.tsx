import { useEffect, useState } from "react";
import playIcon from "./assets/play.png";
import "./DeckPreviewWindow.scss";
import { FlashcardDeck } from "./models/FlashcardDeck";

function WindowTitleBar() {
    return (
        <div className="title-bar">
            <div className="window-controls">
                <div className="button close"></div>
                <div className="button minimize"></div>
                <div className="button maximize"></div>
            </div>
        </div>
    );
}

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

    function playQuestionAudio(): void {
        var audio = document.getElementById("questionAudio") as HTMLAudioElement;
        audio.play();
    }

    function playAnswerAudio(): void {
        var audio = document.getElementById("answerAudio") as HTMLAudioElement;
        audio.play();
    }

    return (
        <section className="flashcards-demo-container">
            <div className="anki-window-frame">
                {WindowTitleBar()}
                <div className="content">
                    <div className="flashcard">
                        <div className="question">{data.flashcards![flashcardIndex].term}</div>
                        <img src={playIcon} alt="" className="playAudio" onClick={playQuestionAudio} />
                        <audio id="questionAudio" src={`./${deckName}/${data.flashcards![flashcardIndex].termAudio}`} />

                        <hr />
                        <div className="answer">{data.flashcards![flashcardIndex].termTranslation}</div>
                        <img src={playIcon} alt="" className="playAudio" onClick={playAnswerAudio} />
                        <audio
                            id="answerAudio"
                            src={`./${deckName}/${data.flashcards![flashcardIndex].termTranslationAudio}`}
                        />
                        <img
                            src={`./${deckName}/${data.flashcards![flashcardIndex].imageCandidates![0]}`}
                            alt=""
                            className="illustration"
                        />
                    </div>
                </div>
                <nav className="bottomBar">
                    <div
                        className="bottomButton previousExample"
                        onClick={() =>
                            setFlashcardIndex(
                                (index) => (((index - 1) % numFlashcards) + numFlashcards) % numFlashcards
                            )
                        }
                    >
                        Previous
                    </div>
                    <div
                        className="bottomButton nextExample"
                        onClick={() => setFlashcardIndex((index) => (index + 1) % numFlashcards)}
                    >
                        Next
                    </div>
                </nav>
            </div>
        </section>
    );
}

export default DeckPreviewWindow;
