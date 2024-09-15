import "./Examples.scss";
import blackImage from "./assets/black.webp";
import audioFileQ from "./assets/brain.mp3";
import audioFileA from "./assets/el-cerebro.mp3";
import playIcon from "./assets/play.svg";

function Example3() {
  const audioQ = new Audio(audioFileQ);
  const audioA = new Audio(audioFileA);

  const playQ = () => audioQ.play();
  const playA = () => audioA.play();

  return (
    <div className="flashcard">
      <div className="question">negro</div>
      <img src={playIcon} alt="" className="playAudio" onClick={playQ} />
      <hr />
      <div className="answer">black</div>
      <img src={playIcon} alt="" className="playAudio" onClick={playA} />
      <img src={blackImage} alt="" className="illustration" />
    </div>
  );
}

export default Example3;
