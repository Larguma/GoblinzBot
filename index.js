const fs = require("node:fs");
const path = require("node:path");
const { Client, Collection, Events, GatewayIntentBits } = require("discord.js");
const { token } = require("./config/config.json");

const client = new Client({
  intents: [
    GatewayIntentBits.Guilds,
    GatewayIntentBits.GuildMessages,
    GatewayIntentBits.MessageContent,
  ],
});

client.commands = new Collection();
const foldersPath = path.join(__dirname, "commands");
const commandFolders = fs.readdirSync(foldersPath);

for (const folder of commandFolders) {
  const commandsPath = path.join(foldersPath, folder);
  const commandFiles = fs
    .readdirSync(commandsPath)
    .filter((file) => file.endsWith(".js"));
  for (const file of commandFiles) {
    const filePath = path.join(commandsPath, file);
    const command = require(filePath);
    if ("data" in command && "execute" in command) {
      client.commands.set(command.data.name, command);
    } else {
      console.log(
        `[WARNING] The command at ${filePath} is missing a required "data" or "execute" property.`
      );
    }
  }
}

client.on(Events.InteractionCreate, async (interaction) => {
  if (!interaction.isChatInputCommand()) return;

  const command = client.commands.get(interaction.commandName);

  if (!command) return;

  try {
    await command.execute(interaction);
  } catch (error) {
    console.error(error);
    if (interaction.replied || interaction.deferred) {
      await interaction.followUp({
        content: "There was an error while executing this command!",
        ephemeral: true,
      });
    } else {
      await interaction.reply({
        content: "There was an error while executing this command!",
        ephemeral: true,
      });
    }
  }
});

// Words list for the "It's Joever" meme
const itsjoever = [
  "over",
  "joever",
  "ado",
  "algo",
  "assembleur",
  "exa",
  "examen",
  "aled",
  "travail écrit",
  "te",
];

// Rock and Stone
const rockandstone = [
  "by the beard",
  "come on guys rock and stone",
  "did i hear a rock and stone?",
  "for karl",
  "for rock and stone",
  "for teamwork",
  "if you don't rock and stone, you ain't comin' home",
  "leave no dwarf behind",
  "like that rock and stone",
  "none can stand before us",
  "rock and roll and stone",
  "rock and roll",
  "rock and stone everyone",
  "rock and stone forever",
  "rock and stone in the heart",
  "rock and stone to the bone",
  "rock and stone",
  "rock and stone, brother",
  "rock and stone... yeeaaahhh",
  "rock on",
  "rock solid",
  "rock... and... stone",
  "stone and rock ...oh, wait...",
  "stone.",
  "that's it lads rock and stone",
  "we are unbreakable",
  "we fight for rock and stone",
  "we rock",
  "yeaahhh rock and stone",
  "yeah, yeah, rock and stone.",
  "https://tenor.com/view/deep-rock-galactic-drg-nonagon-nitra-lootbug-gif-24678517",
  "https://tenor.com/view/deep-rock-galactic-drg-party-rock-and-stone-dancing-gif-17107500",
  "https://tenor.com/view/drg-deep-rock-galactic-gangnam-style-dancing-rock-and-stone-gif-24146010",
  "https://tenor.com/view/drg-deep-rock-galactic-dance-dwarves-rock-and-stone-gif-21655960",
  "https://tenor.com/view/driller-blink-gif-23722040",
  "https://tenor.com/view/rock-and-stone-floss-gif-19745783",
  "https://tenor.com/view/if-you-don%27t-rock-and-stone-you-ain%27t-comin%27-home-deep-rock-galactic-rock-and-stone-run-ganondorf-gif-6106180493101324670",
  "https://tenor.com/view/rock-and-stone-gif-21824862",
  "https://tenor.com/view/deep-rock-galactic-rock-and-stone-gif-22047860",
  "https://tenor.com/view/rock-and-stone-deep-rock-galactic-gif-25114877",
  "https://tenor.com/view/drg-deep-rock-galactic-dwarf-dance-party-gif-21817643"
];

// Check message content
client.on("messageCreate", async (message) => {
  if (message.author.bot) return false;

  const msgContent = message.content.toLowerCase().trim().split(" ");

  // Check word by word
  msgContent.forEach((msg) => {
    if (itsjoever.some((word) => msg == word)) {
      message.reply(
        "https://i.kym-cdn.com/photos/images/newsfeed/002/360/758/f0b.jpg"
      );
    }
  });

  // Check full message
  if (rockandstone.some((word) => message.content.toLowerCase() == word)) {
    message.reply(rockandstone[Math.floor(Math.random() * rockandstone.length)].toUpperCase() + "!");
  }

  // Incite to drink randomly
  if (Math.random() < 0.001) {
    message.reply("Ptite bière !?");
  }
});

client.login(token);
console.log("Bot is ready!");
