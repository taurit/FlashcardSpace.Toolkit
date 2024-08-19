# Anki Card Validator

A proof-of-concept of a ChatGPT-powered tool to validate the quality of Anki cards in your deck.

## The aim of this tool

The goal of this proof-of-concept is to see if current ChatGPT models have good enough efficacy in batch-processing Anki decks and catching:

1) Clear mistakes in Anki cards, such as:

    - Typos in questions or answers.
    - Incorrectly assigned meanings of words
        - This includes cases where the mismatch in meaning is small but there is a better word equivalent

2) More nuanced problems in card content quality. Users might prefer to remove or lower the priority of cards with infrequent usage rates like:

    - Outdated/archaic terms
    - Idioms that are not widely used
    - Regional slang
    - Highly technical jargon
    - Poetic or literary terms
    - Niche, cultural-specific references
    - Highly formal or ceremonial language

## My motivation

I use Anki to learn foreign languages. I have several thousand cards in my queue, the majority of which are imported from some 3rd party sources.

I noticed that many of the imported words are pretty irrelevant to my current learning goals. For example, some contain C2-level jargon I am unlikely ever to use. Time spent memorizing and refining such cards accumulates and is a demotivating factor. I'm happy to remove them or push them to the end of my learning queue, prioritizing basic words (A1-B1) instead.

The tool, however, could be equally useful for cards I create myself while learning, helping me to catch typos and other mistakes.

## Using the tool

I start with a proof-of-concept and an assumption that I'm the only user of the tool.

If you think this tool is something you would like to use, you can let me know by creating a request in `Issues` asking to create a public release of this tool. I might need to clean up the code and remove some hacks to make it work more universally ;)

Thanks for reading!
