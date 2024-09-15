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

    // I added this as an alternative to bundling (commented out above), so there are less changes in git files signalled (except for renames) in the dist directory.
    // this bundles all vendor files into separate file from my user code
    build: {
        rollupOptions: {
            output: {
                manualChunks(id) {
                    if (id.includes("node_modules")) {
                        // All libraries from node_modules go into a 'vendor' chunk
                        return "vendor";
                    }
                },
            },
        },
    },
});
