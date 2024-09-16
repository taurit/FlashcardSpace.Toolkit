import { ReactNode } from "react";
import playIcon from "../assets/play.png";

interface AudioPlayerProps {
    uniqueId: string;
    pathToAudioFile: string;
    replaceDefaultIconWith?: ReactNode;
}

function playAudio(audioElementId: string): void {
    var audio = document.getElementById(audioElementId) as HTMLAudioElement;
    audio.play();
}

const AudioPlayer: React.FC<AudioPlayerProps> = ({ uniqueId, pathToAudioFile, replaceDefaultIconWith }) => {
    let playControl: ReactNode = <img src={playIcon} alt="" className="playAudio" onClick={() => playAudio(uniqueId)} />;

    if (replaceDefaultIconWith) {
        playControl = (
            <div className="playAudioCustom" onClick={() => playAudio(uniqueId)}>
                {replaceDefaultIconWith}
            </div>
        );
    }

    return (
        <>
            {playControl}
            <audio id={uniqueId} src={pathToAudioFile} preload="auto" />
        </>
    );
};

export default AudioPlayer;
