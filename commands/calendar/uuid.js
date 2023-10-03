const { SlashCommandBuilder } = require("discord.js");
const { Level } = require("level");

module.exports = {
  data: new SlashCommandBuilder()
    .setName("uuid")
    .setDescription("List all tasks with UUID"),
  async execute(interaction) {
    const db = new Level("db", { valueEncoding: "json" });
    const entries = await db.iterator().all();
    db.close();

    if (entries.length == 0)
      return interaction.reply({
        content: "You have nothing to do",
        ephemeral: true,
      });

    let out = "";
    for (const [key, value] of entries) {
      if (Date.parse(value.end) > Date.now() - 3 * 24 * 60 * 60 * 1000) {
        out +=
          "**" +
          key +
          "** | " +
          value.end +
          " - " +
          value.lesson +
          ": " +
          value.title +
          "\n";
      }
    }

    return interaction.reply({ content: out, ephemeral: false });
  },
};
