interface ImageProps {
    deckName: string;

    imageCandidates: string[];
    selectedImageIndex: number | undefined;
}

const Image: React.FC<ImageProps> = ({ deckName, imageCandidates, selectedImageIndex }) => {
    if (selectedImageIndex === undefined) return null;
    if (selectedImageIndex < 0 || selectedImageIndex >= imageCandidates.length) {
        return (
            <p className="alert alert-danger">
                selectedImageIndex=`<b>{selectedImageIndex?.toString()}</b>` is out of the expected range <b>[0;&nbsp;{imageCandidates.length})</b>
            </p>
        );
    }

    return <img src={`./${deckName}/${imageCandidates[selectedImageIndex]}`} alt="" className="illustration" />;
};
export default Image;
