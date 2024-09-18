interface ImageProps {
    deckName: string;
    imageCandidates: string[];
}

const Image: React.FC<ImageProps> = ({ deckName, imageCandidates }) => {
    if (imageCandidates && imageCandidates.length > 0) {
        let lastImageIndex = imageCandidates.length - 1;
        return <img src={`./${deckName}/${imageCandidates![lastImageIndex]}`} alt="" className="illustration" />;
    }
};

export default Image;
