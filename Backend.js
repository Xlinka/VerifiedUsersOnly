const express = require('express');
const bodyParser = require('body-parser');
const fs = require('fs');
const app = express();

app.use(bodyParser.json());

const verifiedUsersFile = 'verified-users.txt';

app.get('/verifyuser', (req, res) => {
  const { userId } = req.query;
  if (!userId) {
    console.error('userId parameter is missing');
    res.status(400).json({ error: 'userId parameter is missing' });
    return;
  }
  const verifiedUsers = fs.readFileSync(verifiedUsersFile, 'utf-8').split('\n');
  const isVerified = verifiedUsers.includes(userId);
  console.log(`User ${userId} verification status: ${isVerified}`);
  res.json({ verified: isVerified });
});

const port = 3000;
app.listen(port, () => {
  console.log(`Server listening on port ${port}`);
});
