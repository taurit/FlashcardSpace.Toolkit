import React, { useEffect, useState } from "react";
import PreviewWindow from "./PreviewWindow";
import { FlashcardDeck } from "./models/FlashcardDeck";

const FlashcardLoader: React.FC = () => {
    const deckName = "FlashcardDeck";

    // fetch flashcard data from the `data.json` file
    const [data, setData] = useState<FlashcardDeck | null>(null);
    const [loadingError, setLoadingError] = useState<string | null>(null);

    useEffect(() => {
        fetch(`./${deckName}/flashcards.json`)
            .then((response) => response.json())
            .then((data) => setData(data))
            .catch((error) => {
                setLoadingError(error.message);
            });
    }, []);

    if (loadingError && !data)
        return (
            <div>
                <p>Diagnostic information: couldn't fetch flashcards.json: {loadingError}.</p>
                <br />
            </div>
        );

    return <PreviewWindow deck={data} />;
};

export default FlashcardLoader;
