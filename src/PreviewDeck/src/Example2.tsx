import "./Examples.scss";
import audioFileQ from "./assets/brain.mp3";
import catImage from "./assets/cat.webp";
import audioFileA from "./assets/el-cerebro.mp3";
import playIcon from "./assets/play.svg";

function Example2() {
  const audioQ = new Audio(audioFileQ);
  const audioA = new Audio(audioFileA);

  const playQ = () => audioQ.play();
  const playA = () => audioA.play();

  return (
    <div className="flashcard">
      <div className="question">el gato</div>
      <img src={playIcon} alt="" className="playAudio" onClick={playQ} />
      <hr />
      <div className="answer">a cat</div>
      <img src={playIcon} alt="" className="playAudio" onClick={playA} />
      <img src={catImage} alt="" className="illustration" />
    </div>
  );
}

export default Example2;
