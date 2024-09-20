import "./Toolbar.scss";

const HelloWorld: React.FC = () => {
    const url = window.location.pathname;
    const deckLocalPath = url.substring(0, url.lastIndexOf("/"));

    return (
        <nav className="deck-preview-toolbar">
            <a href={`refineDeck://${deckLocalPath}`} className="deck-preview-toolbar-button" title="Refine the deck">
                ⚙️
            </a>
        </nav>
    );
};

export default HelloWorld;
