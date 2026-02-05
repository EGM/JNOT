// deno run --allow-read --allow-write Code2md.ts

import { walk } from "@std/fs/walk";
import { dirname } from "@std/path/dirname";
import { basename } from "@std/path/basename";

const root = "C:/Users/John/source/repos/JNOT";

async function main() {
  const projectFolders = new Map<string, string>(); // name -> full path

  // 1. Find ALL .csproj files anywhere under root
  for await (const entry of walk(root, {
    includeDirs: false,
    includeFiles: true,
    followSymlinks: false,
    skip: [/[/\\](bin|obj|\.git|\.vs)(?=$|[/\\])/],
  })) {
    if (entry.path.endsWith(".csproj")) {
      const folder = dirname(entry.path);
      const name = basename(folder);
      projectFolders.set(name, folder);
    }
  }

  console.log("Detected projects:", [...projectFolders.keys()]);

  // 2. Generate <project>.md for each project folder
  for (const [projectName, projectPath] of projectFolders) {
    const chunks: string[] = [];

    for await (
      const entry of walk(projectPath, {
        includeDirs: true,
        includeFiles: true,
        followSymlinks: false,
        skip: [/[/\\](bin|obj|\.git|\.vs)(?=$|[/\\])/],
      })
    ) {
      if (entry.isDirectory) {
        const rel =
          entry.path === projectPath
            ? "/"
            : entry.path.slice(projectPath.length + 1);
        chunks.push(`\n## 📁 Directory: ${rel}\n`);
        continue;
      }

      // Skip dot-folders
      if (entry.path.split(/[/\\]/).some((p) => p.startsWith("."))) {
        continue;
      }

      const relPath = entry.path.slice(projectPath.length + 1);
      chunks.push(`- ${relPath}`);

      if (entry.path.endsWith(".cs")) {
        const text = await Deno.readTextFile(entry.path);
        chunks.push("\n```cs");
        chunks.push(text);
        chunks.push("```\n");
      }
    }

    const outFile = `${projectName}.md`;
    await Deno.writeTextFile(outFile, chunks.join("\n"));
    console.log("Wrote:", outFile);
  }

  console.log("All project logs complete.");
}

main();