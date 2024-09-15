import React from "react";
import ReactDOM from "react-dom/client";
import Demo from "./Demo.tsx";

// added to make debugging in Firefox responsibe/mobile rendering mode easier
const codeToInjectWhenRanAsStandaloneApp = import.meta.env.DEV && (
  <meta name="viewport" content="width=device-width, initial-scale=1" />
);

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    {codeToInjectWhenRanAsStandaloneApp}
    <Demo />
  </React.StrictMode>
);
