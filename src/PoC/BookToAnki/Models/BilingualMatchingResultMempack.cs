using BookToAnki.Services;
using MemoryPack;

namespace BookToAnki.Models;

[MemoryPackable]
partial record BilingualMatchingResultMempack(BilingualSentenceMatchingResult EnUk, BilingualSentenceMatchingResult EnPl);
