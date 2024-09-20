import React, { useEffect, useState } from "react";
import PreviewWindow from "./PreviewWindow";
import Toolbar from "./Toolbar";
import { FlashcardDeck } from "./models/FlashcardDeck";

const FlashcardLoader: React.FC = () => {
    const deckName = "FlashcardDeck";

    // fetch flashcard data from the `data.json` file
    const [data, setData] = useState<FlashcardDeck | null>(null);
    const [loadingError, setLoadingError] = useState<string | null>(null);
    const [isInWpfIntegrationMode, setIsInWpfIntegrationMode] = useState<boolean | null>(true);

    useEffect(() => {
        (window as any).setDataFromWpf = setData;
        return () => {
            delete (window as any).setDataFromWpf;
        };
    }, []);

    useEffect(() => {
        fetch(`./${deckName}/flashcards.json`)
            .then((response) => response.json())
            .then((data) => setData(data))
            .then(() => {
                setIsInWpfIntegrationMode(false);
            })
            .catch((error) => {
                setLoadingError(error.message);
            });
    }, []);

    if (loadingError && !data)
        return (
            <div>
                data: {data}
                <p>
                    <strong>Select the flashcard to see the preview.</strong>
                </p>
                <p>Diagnostic information: WPF integration mode assumed because couldn't fetch flashcards.json: {loadingError}.</p>
                <br />
            </div>
        );

    return (
        <>
            <PreviewWindow deck={data} />
            {!isInWpfIntegrationMode && <Toolbar />}
        </>
    );
};

export default FlashcardLoader;
