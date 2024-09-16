interface ImageProps {
    deckName: string;
    imageCandidates: string[];
}

const Image: React.FC<ImageProps> = ({ deckName, imageCandidates }) => {
    if (imageCandidates && imageCandidates.length > 0) {
        return <img src={`./${deckName}/${imageCandidates![0]}`} alt="" className="illustration" />;
    }
};

export default Image;
