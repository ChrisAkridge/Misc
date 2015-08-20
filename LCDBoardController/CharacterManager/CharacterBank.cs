using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace CharacterManager
{
	public sealed class CharacterBank
    {
		private Dictionary<string, CustomCharacter> characters;

		public ReadOnlyDictionary<string, CustomCharacter> Characters
		{
			get
			{
				return new ReadOnlyDictionary<string,CustomCharacter>(characters);
			}
		}

		public CustomCharacter this[string name]
		{
			get
			{
				if (characters.ContainsKey(name))
				{
					return characters[name];
				}
				return null;
			}
		}

		public CharacterBank(string filename)
		{
			characters = new Dictionary<string,CustomCharacter>();

			if (!File.Exists(filename))
			{
				throw new FileNotFoundException();
			}

			using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
			{
				uint numberOfCharacters = reader.ReadUInt32();

				for (uint i = 0u; i < numberOfCharacters; i++)
				{
					ushort characterNameLength = reader.ReadUInt16();
					byte[] characterNameBytes = reader.ReadBytes(characterNameLength);
					string characterName = Encoding.UTF8.GetString(characterNameBytes);
					byte[] patternBytes = reader.ReadBytes(8);

					characters.Add(characterName, new CustomCharacter(patternBytes));
				}
			}
		}

		public void Add(string name, CustomCharacter character)
		{
			characters.Add(name, character);
		}

		public void Remove(string name)
		{
			characters.Remove(name);
		}

		public void WriteToFile(string filename)
		{
			File.WriteAllText(filename, "");

			using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(filename)))
			{
				writer.Write((uint)characters.Count);

				foreach (var character in characters)
				{
					byte[] utf8Name = Encoding.UTF8.GetBytes(character.Key);
					writer.Write((ushort)utf8Name.Length);
					writer.Write(utf8Name);
					writer.Write(character.Value.Pattern);
				}
			}
		}
    }
}
