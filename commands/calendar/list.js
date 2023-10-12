const { SlashCommandBuilder } = require("discord.js");
const { Level } = require("level");

module.exports = {
  data: new SlashCommandBuilder().setName("list").setDescription("List tasks"),
  async execute(interaction) {
    const db = new Level("db", { valueEncoding: "json" });
    const entries = await db.iterator().all();
    db.close();
    if (entries.length == 0)
      return interaction.reply({
        content: "You have nothing to do",
        ephemeral: true,
      });
    let items = [];
    for (const [key, value] of entries) {
      items.push(value);
    }

    let out = "## TODO\n```ansi\n";
    for (const value of items.sort((x, y) => (x.end > y.end ? 1 : -1))) {
      if (Date.parse(value.end) > Date.now() - 3 * 24 * 60 * 60 * 1000) {
        if (Date.parse(value.end) < Date.now() - 1 * 24 * 60 * 60 * 1000) out += "[2;31m";
        else if (Date.parse(value.end) < Date.now()) out += "[2;33m";
        else if (value.exa == "true") out += "[2;32m";
        else out += "[2;30m[2;37m";
        out += value.end + " - " + value.lesson + ": " + value.title + "\n";
      }
    }
    out += "```";

    return interaction.reply({ content: out, ephemeral: false });
  },
};
