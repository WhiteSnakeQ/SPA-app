export const GET_COMMENTS = `
query GetComments($input: GetCommentsQueryInput!)
{
	comments(input: $input)
	{
		hasNextPage

		items
		{
			id
			text
			userName
			email
			createdAt

			replyCount

			files
			{
				id
				fileUrl
				fileName
				fileType
			}
		}
	}
}`

export const GET_REPLY = `
query GetReplyComments($input: GetReplyCommentsQueryInput!)
{
    replyComments(input: $input)
    {
        id
		parentId
		text
		userName
		email
		createdAt

		replyCount

		files
		{
      		id
			fileUrl
			fileName
			fileType
		}
    }
}`

export function buildReplyCommentsQuery(depth: number): string
{
	return `query GetReplyComments($input: GetReplyCommentsQueryInput!)
	{
		replyComment(input: $input)
		{
			id
			parentId
			text
			userName
			email
			createdAt

			${buildChildren(depth)}
		}
	}`;
}

function buildChildren(depth: number): string
{
    if (depth <= 0)
        return '';

    return `
        children
        {
            id
            parentId
            text
            userName
            email
            createdAt

            ${buildChildren(depth - 1)}
        }
    `;
}