import "./Examples.scss";
import audioFileQ from "./assets/brain.mp3";
import notebookImage from "./assets/brain.webp";
import audioFileA from "./assets/el-cerebro.mp3";
import playIcon from "./assets/play.svg";

function Example1() {
  const audioQ = new Audio(audioFileQ);
  const audioA = new Audio(audioFileA);

  const playQ = () => audioQ.play();
  const playA = () => audioA.play();

  return (
    <div className="flashcard">
      <div className="question">brain</div>
      <img src={playIcon} alt="" className="playAudio" onClick={playQ} />
      <hr />
      <div className="answer">el cerebro</div>
      <img src={playIcon} alt="" className="playAudio" onClick={playA} />
      <img src={notebookImage} alt="" className="illustration" />
    </div>
  );
}

export default Example1;
