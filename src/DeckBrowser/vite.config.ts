import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";
import { viteSingleFile } from "vite-plugin-singlefile";

// https://vitejs.dev/config/
/** @type {import('vite').UserConfig} */
export default defineConfig({
    plugins: [react(), viteSingleFile()],
    //base: "/demo/", // use this if you want to serve from a subfolder. Also, add it in tasks.json or relative URLs won't work as expected!
    base: "/",
    esbuild: { legalComments: "none" },
    css: {
        preprocessorOptions: {
            scss: {
                // Added to avoid warnings; https://stackoverflow.com/a/79003101/889779
                api: "modern",
            },
        },
    },
});
