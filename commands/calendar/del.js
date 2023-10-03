const { SlashCommandBuilder } = require("discord.js");
const { Level } = require("level");

module.exports = {
  data: new SlashCommandBuilder()
    .setName("del")
    .setDescription("Delete a task")
    .addStringOption((option) =>
      option
        .setName("uuid")
        .setDescription("The UUID of the task (/uuid)")
        .setRequired(true)
    ),

  async execute(interaction) {
    const uuid = interaction.options.getString("uuid");

    const db = new Level("db", { valueEncoding: "json" });
    try {
      await db.del(uuid);
      db.close();
      return interaction.reply({
        content: "Task removed (" + uuid + ")",
        ephemeral: false,
      });
    } catch {
      db.close();
      return interaction.reply({ content: "ERROR", ephemeral: true });
    }
  },
};
