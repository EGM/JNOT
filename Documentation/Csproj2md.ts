import { walk } from "@std/fs/walk";

async function convertCsprojToMarkdown(directory: string) {
    for await (const entry of walk(directory)) {
        if (entry.isFile && entry.name.endsWith(".csproj")) {
            try {
                const csprojContent = await Deno.readTextFile(entry.path);
                const mdContent = `\`\`\`xml\n${csprojContent}\n\`\`\``;
                
                const mdFilePath = entry.path.replace(/\.csproj$/, ".csproj.md");
                await Deno.writeTextFile(mdFilePath, mdContent);
                console.log(`Converted: ${entry.path} to ${mdFilePath}`);
            } catch (error) {
                console.error(`Error processing ${entry.path}:`, error);
            }
        }
    }
}

// Usage
const startDirectory = "C:/Users/John/source/repos/Jnot";
convertCsprojToMarkdown(startDirectory);
