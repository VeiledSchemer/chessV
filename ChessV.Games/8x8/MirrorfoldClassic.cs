/***************************************************************************
 *
 *  MirrorfoldClassic.cs
 *
 *  A custom variant for ChessV 2.2 – Mirrorfold Classic.
 *
 *  Board: 6x6
 *  Pieces: Each side has 6 pieces chosen from {K, R, N, B, P, P}.
 *          White’s back rank is randomized; Black’s back rank is the
 *          mirror image (reversed order and lowercase) of White’s.
 *
 *  Victory: Default victory (king capture) – additional rules can be added.
 *
 *  COPYRIGHT (C) 2023 by [Your Name or Your Alias]
 *
 *  This code is released under the GNU General Public License.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using ChessV.Games;         // base classes and attributes
using ChessV.Geometry;      // for board geometry

namespace ChessV.Games
{
    [Game("Mirrorfold Classic", typeof(Rectangular), 6, 6,
          InventedBy = "Your Name",
          Invented = "2023",
          Tags = "Chess Variant,Random,Mirrorfold",
          GameDescription1 = "A 6x6 variant in which each side’s back rank is randomly generated from {K, R, N, B, P, P}.",
          GameDescription2 = "Black’s rank is the horizontal mirror (reverse order, lowercase) of White’s.")]
    class MirrorfoldClassic : Chess
    {
        [GameVariable]
        public IntRangeVariable PositionNumber { get; set; }

        // Cache the list of unique permutations so we compute it only once.
        static List<string> _uniquePermutations = null;

        public MirrorfoldClassic()
        {
            // Constructor – additional initialization can be done here.
        }

        public override void SetGameVariables()
        {
            base.SetGameVariables();
            // Set PositionNumber to range from 1 to 360 (360 unique arrangements for {K,R,N,B,P,P})
            PositionNumber = new IntRangeVariable(1, 360);
            // Disable castling for this variant (or set any other game-wide rules)
            Castling.Value = "None";
            Array = null; // Ensure that LookupGameVariable("ARRAY") will be used.
        }

        /// <summary>
        /// Overrides the lookup for the "ARRAY" game variable, which provides the starting position.
        /// For Mirrorfold Classic, we generate a FEN string for a 6x6 board where the only populated ranks
        /// are the first (White’s) and the sixth (Black’s). White’s back rank is a random permutation (from
        /// the chosen PositionNumber) of {K, R, N, B, P, P} and Black’s rank is its mirror image.
        /// </summary>
        public override object LookupGameVariable(string variableName)
        {
            if (variableName.ToUpper() == "ARRAY")
            {
                // Get the selected index (1-indexed)
                int posIndex = (int)PositionNumber.Value; // valid values: 1 to 360

                // Generate (or retrieve cached) list of all unique permutations of the multiset {K,R,N,B,P,P}.
                List<string> perms = GenerateUniquePermutations();

                // Select the permutation for white's back rank.
                // Convert to uppercase to conform with standard FEN convention.
                string whiteRank = perms[posIndex - 1].ToUpper();

                // Black's back rank is the horizontal mirror: reverse the whiteRank string and convert to lowercase.
                string blackRank = ReverseString(whiteRank).ToLower();

                // For a 6x6 board, an empty rank is represented by "6".
                string emptyRank = "6";

                // Construct the FEN string.
                // Note: FEN is specified from the top rank (rank 6, Black's back rank) to the bottom (rank 1, White's).
                // The FEN fields are: [board] [side-to-move] [castling] [en passant] [halfmove clock] [fullmove number]
                string fen = $"{blackRank}/{emptyRank}/{emptyRank}/{emptyRank}/{emptyRank}/{whiteRank} w - - 0 1";

                return fen;
            }
            // For any other variable, defer to the base implementation.
            return base.LookupGameVariable(variableName);
        }

        /// <summary>
        /// Recursively generate all unique permutations of the multiset "K, R, N, B, P, P"
        /// and cache the sorted list.
        /// </summary>
        private List<string> GenerateUniquePermutations()
        {
            if (_uniquePermutations != null)
                return _uniquePermutations;

            // Define the multiset as a list of characters.
            List<char> items = new List<char> { 'K', 'R', 'N', 'B', 'P', 'P' };
            HashSet<string> permsSet = new HashSet<string>();
            Permute(items, 0, permsSet);
            List<string> permsList = permsSet.ToList();
            permsList.Sort();
            _uniquePermutations = permsList;
            return permsList;
        }

        /// <summary>
        /// Generates all permutations of the list 'arr' starting at index 'start' and
        /// adds each unique permutation (as a string) to the provided set.
        /// </summary>
        private void Permute(List<char> arr, int start, HashSet<string> perms)
        {
            if (start >= arr.Count)
            {
                perms.Add(new string(arr.ToArray()));
                return;
            }
            // Use a hash set to avoid swapping in duplicate characters.
            HashSet<char> swapped = new HashSet<char>();
            for (int i = start; i < arr.Count; i++)
            {
                if (swapped.Contains(arr[i]))
                    continue;
                swapped.Add(arr[i]);
                Swap(arr, start, i);
                Permute(arr, start + 1, perms);
                Swap(arr, start, i); // backtrack
            }
        }

        private void Swap(List<char> arr, int i, int j)
        {
            char temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }

        private string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        public override void AddRules()
        {
            // Call the base rules (which include the default victory condition on king capture).
            base.AddRules();
            
            // If desired, add additional victory conditions here (for example, if occupying the enemy back rank
            // or other special conditions should trigger a win, insert the necessary rules or game state checks).
        }
    }
}
