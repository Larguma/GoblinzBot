const { SlashCommandBuilder, Events, ModalBuilder } = require('discord.js');
const { Level } = require('level');

module.exports = {
  data: new SlashCommandBuilder()
  .setName('add')
  .setDescription('Add a task')
  .addStringOption(option =>
		option.setName('career')
			.setDescription('Who ?')
			.setRequired(true)
			.addChoices(
				{ name: 'IL', value: 'IL' },
        { name: 'RS', value: 'RS' },
        { name: 'BOTH', value: 'BOTH' },
			))
  .addStringOption(option => option.setName('lesson').setDescription('Lesson ? (Maths)').setRequired(true))
  .addStringOption(option => option.setName('title').setDescription('Task ? (Ex. 1-2-3)').setRequired(true))
  .addStringOption(option => option.setName('end').setDescription('When ? (yyyy-mm-dd)').setRequired(true))
  .addStringOption(option =>
    option.setName('exa')
    .setDescription('Is exa ?')
    .addChoices(
      { name: 'False', value: 'false' },
      { name: 'True', value: 'true' },
    )),
  
	async execute(interaction) {
    const career = interaction.options.getString('career');
    console.log(career)
    const title = interaction.options.getString('title');
    const lesson = interaction.options.getString('lesson');
    const end = interaction.options.getString('end');
    const exa = interaction.options.getString('exa');
    if (isNaN(Date.parse(end))) return interaction.reply({ content: "Not a valid date", ephemeral: true });

    const db = new Level('db', { valueEncoding: 'json' })
    try {
     await db.put(uuidv4(), {"career": career, "title": title, "lesson": lesson, "end": end, "exa": exa})
     db.close()
     return interaction.reply({ content: "Task added (" + lesson + ")" , ephemeral: false });
    }
    catch {
      console.log(career, title, lesson, end)
      db.close()
      return interaction.reply({ content: "ERROR", ephemeral: true });
    }
	},
};

function uuidv4() {
    return 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'
    .replace(/[xy]/g, function (c) {
        const r = Math.random() * 16 | 0, 
            v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}