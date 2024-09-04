# Sources of data

## Polish.Words.txt

- Source: [wikipedia-word-frequency](https://github.com/IlyaSemenov/wikipedia-word-frequency/blob/5a6768888388d4455447ac7c760e940f4f7c1304/results/plwiki-2022-08-29.txt)
- Processing performed to adapt to this project: file was trimmed to leave only the first 171 000 lines, which corresponds to 95% of word occurences in the original dictionary. This gets rid of mostly totally useless words (proper names, weird and rarely used jargon), while significantly the size of dictionary and making it more suitable to commit to a repository.
- Remarks: quality might be problematic, e.g., with some words prioritized too high over real world usage, but maybe it's good enough for the top 3000 at least? At least licensing issues seem unlikely.

## Spanish.Words.RAE.txt

- Source: [Real Academia Española](<https://corpus.rae.es/lfrecuencias.html>)
- Processing performed to adapt to this project: Changed encoding from windows-1252 to UTF-8. Trimmed to leave only the first 48 000 lines.

## Spanish.Words.txt

- Source: [wikipedia-word-frequency](https://github.com/IlyaSemenov/wikipedia-word-frequency/blob/5a6768888388d4455447ac7c760e940f4f7c1304/results/eswiki-2022-08-29.txt)
- Processing performed to adapt to this project: file was trimmed to leave only the first 48 000 lines, which corresponds to 95% of word occurences in the original dictionary.
- Remarks: `Spanish.Words.txt` probably has higher quality, as it was reviewed and published by an institution.

## English.Words.txt

- Source: [wikipedia-word-frequency](https://github.com/IlyaSemenov/wikipedia-word-frequency/blob/5a6768888388d4455447ac7c760e940f4f7c1304/results/enwiki-2023-04-13.txt)
- Processing performed to adapt to this project: file was trimmed to leave only the first 51 000 lines, which corresponds to 95% of word occurences in the original dictionary.
