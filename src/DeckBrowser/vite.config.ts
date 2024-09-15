import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        react(),
        // viteSingleFile() // this is mostly for aesthetics; not sure whether to turn it on or off yet. Git diffs are cleaner when it's off.
    ],
    //base: "/demo/", // use this if you want to serve from a subfolder. Also, add it in tasks.json or relative URLs won't work as expected!
    base: "/",
    esbuild: { legalComments: "none" },
});
