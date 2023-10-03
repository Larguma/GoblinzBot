const { SlashCommandBuilder } = require("discord.js");
const { Level } = require("level");

module.exports = {
  data: new SlashCommandBuilder()
    .setName("listall")
    .setDescription("List all tasks"),
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

    let out = "```diff\n";
    for (const value of items.sort((x, y) => (x.end > y.end ? 1 : -1))) {
      if (Date.parse(value.end) < Date.now()) out += "- ";
      else out += "  ";
      out += value.end + " - " + value.lesson + ": " + value.title + "\n";
    }
    out += "```";

    return interaction.reply({ content: out, ephemeral: false });
  },
};
