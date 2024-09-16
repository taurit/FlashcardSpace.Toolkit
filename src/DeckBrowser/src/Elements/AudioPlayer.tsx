import playIcon from "../assets/play.png";

interface AudioPlayerProps {
    uniqueId: string;
    pathToAudioFile: string;
}

function playAudio(audioElementId: string): void {
    var audio = document.getElementById(audioElementId) as HTMLAudioElement;
    audio.play();
}

const AudioPlayer: React.FC<AudioPlayerProps> = ({ uniqueId, pathToAudioFile }) => {
    return (
        <>
            <img src={playIcon} alt="" className="playAudio" onClick={() => playAudio(uniqueId)} />
            <audio id={uniqueId} src={pathToAudioFile} preload="auto" />
        </>
    );
};

export default AudioPlayer;
