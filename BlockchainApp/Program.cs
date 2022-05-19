﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static BlockchainApp.Program;

namespace BlockchainApp
{
    public static class BlockChainExtension
    {
        public static byte[] GenerateHash(this IBlock block)
        {
            using (SHA512 sha = new SHA512Managed())
            using (MemoryStream st = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(st))
            {
                bw.Write(block.Data);
                bw.Write(block.Nonce);
                bw.Write(block.TimeStamp.ToBinary());
                bw.Write(block.PrevHash);
                var starr = st.ToArray();
                return sha.ComputeHash(starr);
            }
        }
        public static byte[] MineHash(this IBlock block, byte[] difficulty)
        {
            if (difficulty == null) throw new ArgumentNullException(nameof(difficulty));

            byte[] hash = new byte[0];
            int d = difficulty.Length;
            while (!hash.Take(2).SequenceEqual(difficulty))
            {
                block.Nonce++;
                hash = block.GenerateHash();
            }
            return hash;
        }
        public static bool IsValid(this IBlock block)
        {
            var bk = block.GenerateHash();
            return block.Hash.SequenceEqual(bk);
        }
        public static bool[] IsValidPrevBlock(this IBlock block, IBlock prevBlock)
        {
            if (prevBlock == null) throw new ArgumentNullException(nameof(prevBlock));
            return prevBlock.IsValid() && block.PrevHash.SequenceEqual(prev);

        }
        public static bool IsValid(this IEnumerable<IBlock> blocks)
        {
            var enmerable = items.ToList();
            return enmerable.Zip(enmerable.Skip(1), Tuple.Create).All(block => block.Item2.IsValid() && block.Item2.IsValidPrevBlock(block.Item1));
        }
    }
    public interface IBlock
    {
        byte[] Data { get; }
        byte[] Hash { get; set; }
        int Nonce { get; set; }
        byte[] PrevHash { get; set; }
        DateTime TimeStamp { get; }
    }
    public class Block : IBlock
    {
        public Block(byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Nonce = 0;
            PrevHash = new byte[] { 0x00 };
            TimeStamp = DateTime.Now;
        }

        public byte[] Data { get; }
        public byte[] Hash { get; set; }
        public int Nonce { get; set; }
        public byte[] PrevHash { get; set; }
        public DateTime TimeStamp { get; } 


        public override string ToString()
        {
            return $"{BitConverter.ToString(Hash).Replace("-", "")}:\n {BitConverter.ToString(PrevHash).Replace("-", "")}\n {Nonce},{TimeStamp}";
        }
    }
    public class Blockchain : IEnumerable<IBlock>
    {
        private List<IBlock> _items = new List<IBlock>();

        public Blockchain(byte[] difficulty, IBlock genisis)
        {
            Difficulty = difficulty;
            genisis.Hash = genisis.MineHash(difficulty);
            Items.Add(genisis);
        }

        public void Add(IBlock item)
        {
            if (Items.LastOrDefault != null)
            {
                item.PrevHash = Items.LastOrDefault()?.Hash;
            }
            item.Hash = item.MineHash(Difficulty);
            Items.Add(item);
        }

        public int Count => _items.Count;
        public IBlock this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }
        public List<IBlock> Items
        {
            get => _items;
            set => _items = value;
        }
        public byte[] Difficulty { get; }

        public IEnumerator<IBlock> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            IBlock genesis = new Block(new byte[] {0x00, 0x00 , 0x00 , 0x00 , 0x00 });
            byte[] difficulty = new byte[] { 0x00, 0x00 };

            Blockchain chain = new Blockchain(difficulty, genesis);
            for(int i = 0; i < 200; i++)
            {
                var data = Enumerable.Range(0, 2256).Select(p => (byte)rand.Next());
                chain.Add(new Block(data.ToArray()));
                Console.WriteLine(chain.LastOrDefault()?.ToString());

                Console.WriteLine($"Chain is Valid: {chain.IsValid()}");
            }

            Console.ReadLine();
        }
    }
}
