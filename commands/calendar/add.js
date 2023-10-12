const { SlashCommandBuilder, Events, ModalBuilder } = require("discord.js");
const { Level } = require("level");

module.exports = {
  data: new SlashCommandBuilder()
    .setName("add")
    .setDescription("Add a task")
    .addStringOption((option) =>
      option
        .setName("lesson")
        .setDescription("Lesson ?")
        .setRequired(true)
        .addChoices(
          { name: "Admin", value: "Admin" },
          { name: "Ado (A)", value: "Ado (A)" },
          { name: "Ado (B)", value: "Ado (B)" },
          { name: "Algo", value: "Algo" },
          { name: "Concurp (A)", value: "Concurp (A)" },
          { name: "Concurp (B)", value: "Concurp (B)" },
          { name: "DevOps", value: "DevOps" },
          { name: "Maths", value: "Maths" },
          { name: "Mobile", value: "Mobile" },
          { name: "Physique", value: "Physique" },
          { name: "Projet", value: "Projet" },
          { name: "Stats", value: "Stats" },
          { name: "SysInfo", value: "SysInfo" }
        )
    )
    .addStringOption((option) =>
      option
        .setName("title")
        .setDescription("Task ? (Ex. 1-2-3)")
        .setRequired(true)
    )
    .addStringOption((option) =>
      option
        .setName("end")
        .setDescription("When ? (yyyy-mm-dd)")
        .setRequired(true)
    )
    .addStringOption((option) =>
      option
        .setName("exa")
        .setDescription("Is exa ?")
        .addChoices(
          { name: "False", value: "false" },
          { name: "True", value: "true" }
        )
    ),

  async execute(interaction) {
    const title = interaction.options.getString("title");
    const lesson = interaction.options.getString("lesson");
    const end = interaction.options.getString("end");
    const exa = interaction.options.getString("exa");
    if (isNaN(Date.parse(end)))
      return interaction.reply({
        content: "Not a valid date",
        ephemeral: true,
      });

    const db = new Level("db", { valueEncoding: "json" });
    try {
      await db.put(uuidv4(), {
        lesson: lesson,
        title: title,
        end: end,
        exa: exa,
      });
      db.close();
      return interaction.reply({
        content: "Task added (" + lesson + ")",
        ephemeral: false,
      });
    } catch {
      console.log(title, lesson, end);
      db.close();
      return interaction.reply({ content: "ERROR", ephemeral: true });
    }
  },
};

function uuidv4() {
  return "xxxx".replace(/[xy]/g, function (c) {
    const r = (Math.random() * 16) | 0,
      v = c == "x" ? r : (r & 0x3) | 0x4;
    return v.toString(16);
  });
}
