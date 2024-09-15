import { useEffect, useState } from "react";
import "./Demo.scss";
import ankiIcon from "./assets/anki-icon.svg";

function WindowTitleBar() {
  return (
    <div className="title-bar">
      <div className="title">
        <img src={ankiIcon} alt="" className="appIcon" />
        <span>Anki</span>
      </div>
      {/* <div className="window-controls">
        <div className="button close">⨉</div>
      </div> */}
    </div>
  );
}

class Flashcard {
  public frontText: string | undefined;
  public frontAudio: string | undefined;

  public backText: string | undefined;
  public backAudio: string | undefined;
}

function Demo() {
  // fetch flashcard data from the `data.json` file
  const [data, setData] = useState<Flashcard[] | null>(null);
  useEffect(() => {
    fetch("./flashcards-example.json")
      .then((response) => response.json())
      .then((data) => setData(data));
  }, []);

  // display the flashcard data
  if (data === null) return null;
  return (
    <section className="flashcards-demo-container">
      <div className="anki-window-frame">
        {WindowTitleBar()}
        <div className="content">
          <div className="flashcard">
            <div className="front">
              <div className="text">{data[0].frontText}</div>
              <audio src={data[0].frontAudio} controls />
            </div>
            <div className="back">
              <div className="text">{data[0].backText}</div>
              <audio src={data[0].backAudio} controls />
            </div>
          </div>
        </div>
        <nav className="bottomBar">
          <div className="bottomButton previousExample">Previous</div>
          <div className="bottomButton nextExample">Next</div>
        </nav>
      </div>
    </section>
  );

  // const [exampleIndex, setExampleIndex] = useState(0);
  // const numExamples = 2;

  // return (
  //   <section className="flashcards-demo-container">
  //     <div className="anki-window-frame">
  //       {WindowTitleBar()}
  //       <div className="content">
  //         {exampleIndex == 0 ? <Example2 /> : <Example3 />}
  //       </div>
  //       <nav className="bottomBar">
  //         <div
  //           className="bottomButton previousExample"
  //           onClick={() =>
  //             setExampleIndex((index) => (index - 1) % numExamples)
  //           }
  //         >
  //           Previous
  //           {/* Previous example */}
  //         </div>
  //         <div
  //           className="bottomButton nextExample"
  //           onClick={() =>
  //             setExampleIndex((index) => (index + 1) % numExamples)
  //           }
  //         >
  //           Next
  //           {/* Next example */}
  //         </div>
  //       </nav>
  //     </div>
  //   </section>
  // );
}

export default Demo;
